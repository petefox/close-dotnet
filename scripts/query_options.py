"""Generate additive filter option types from Close's parameter definitions."""

import html
import re

CORE_OPTIONS = {
    "contacts_list": "ContactListOptions",
    "opportunities_list": "OpportunityListOptions",
    "tasks_list": "TaskListOptions",
    "activities.notes_list": "NoteListOptions",
    "events_list": "CloseEventListOptions",
}
EVENT_OPTIONS = {"_cursor", "_limit", "object_type", "action", "date_updated__gte"}
COMPARISONS = {"gt": "After", "gte": "OnOrAfter", "lt": "Before", "lte": "OnOrBefore"}
PROPERTY_NAMES = {"query": "SearchQuery", "s_query": "AdvancedSearchQuery", "_type": "Type"}
DOMAIN_ENUMS = {
    ("opportunities_list", "status_type"): "OpportunityStatusType",
    ("opportunities_list", "status_type__in"): "OpportunityStatusType",
    ("opportunities_list", "value_period"): "OpportunityValuePeriod",
    ("opportunities_list", "value_period__in"): "OpportunityValuePeriod",
}
# The task format filter selects an iCalendar response; the typed task methods return JSON.
EXCLUDED = {("tasks_list", "format")}


class QueryOptions:
    def __init__(self, spec, pascal, group_name, method_name, literal):
        self.spec = spec
        self.pascal = pascal
        self.group_name = group_name
        self.method_name = method_name
        self.literal = literal
        self.classes = {}
        self.enums = {}

    def resolve(self, node):
        while "$ref" in node:
            target = self.spec
            for key in node["$ref"][2:].split("/"):
                target = target[key]
            node = target
        return node

    def variants(self, node):
        reference = node.get("$ref", "").rsplit("/", 1)[-1]
        node = self.resolve(node)
        combined = node.get("anyOf", node.get("oneOf"))
        if combined:
            variants = [item for part in combined for item in self.variants(part)]
            if reference and len(variants) == 1:
                return [(variants[0][0], reference)]
            return variants
        types = node.get("type")
        if not isinstance(types, list):
            types = [types]
        return [(dict(node, type=kind), reference) for kind in types if kind != "null"]

    def parameters(self, op):
        return [p for node in op.get("parameters", []) if (p := self.resolve(node)).get("in") == "query"]

    def enum(self, name, values):
        members = {}
        for value in values:
            member = self.pascal(value)
            if value.startswith("-"):
                member += "Descending"
            if member in members:
                raise ValueError(f"Conflicting enum member {name}.{member}")
            members[member] = value
        if name in self.enums and self.enums[name] != members:
            raise ValueError(f"Conflicting enum {name}")
        self.enums[name] = members
        return name

    def field(self, op_id, parameter, options_name):
        wire = parameter["name"]
        stem, _, suffix = wire.partition("__")
        prop = PROPERTY_NAMES.get(wire, self.pascal(stem))
        if suffix == "in":
            prop += "s" if not prop.endswith("s") else "In"
        elif suffix:
            prop += COMPARISONS.get(suffix, self.pascal(suffix))

        variants = self.variants(parameter.get("schema", {}))
        kinds = {shape.get("type") for shape, _ in variants}
        if not variants:
            return None
        shape, reference = variants[0]
        format_name = shape.get("format")
        description = parameter.get("description") or shape.get("description", "")
        csv = suffix == "in" or "comma-separated" in description.lower()
        is_sequence = suffix == "in" or kinds == {"array"}
        if kinds == {"array"}:
            variants = self.variants(shape.get("items", {}))
            if len(variants) != 1:
                return None
            shape, reference = variants[0]
            kinds = {shape.get("type")}

        domain_enum = DOMAIN_ENUMS.get((op_id, wire))
        if domain_enum:
            shape, reference = self.spec["components"]["schemas"][domain_enum], domain_enum
        if kinds == {"string"} and (values := shape.get("enum")) and all(isinstance(v, str) for v in values):
            value_type = self.enum(reference or options_name.removesuffix("Options") + prop, values)
            accessor = (f"{prop}?.Select(CloseQueryEnums.Format).ToArray()" if is_sequence
                        else f"{prop} is {{ }} value{prop} ? CloseQueryEnums.Format(value{prop}) : null")
        else:
            formats = {v.get("format") for v, _ in variants}
            if op_id == "opportunities_list" and stem == "is_stalled":
                value_type = "bool"
            elif kinds == {"string"} and (({"date", "date-time"} <= formats) or "ISO 8601 date or timestamp" in description):
                value_type = "CloseFilterDate"
            elif kinds == {"string"} and (format_name == "date" or op_id == "opportunities_list" and stem == "date_won"):
                value_type = "DateOnly"
            elif kinds == {"string"} and (format_name == "date-time" or op_id == "events_list" and stem == "date_updated"):
                value_type = "DateTimeOffset"
            elif kinds == {"string"}:
                value_type = "string"
            elif kinds == {"integer"}:
                value_type = "int" if wire in {"_limit", "_skip", "entry", "limit", "results_limit"} else "long"
            elif kinds == {"number"}:
                value_type = "decimal"
            elif kinds == {"boolean"}:
                value_type = "bool"
            else:
                return None
            if is_sequence and value_type != "string":
                return None
            accessor = prop

        if is_sequence and suffix != "in" and not prop.endswith("s"):
            prop += "s"
            accessor = accessor.replace(self.pascal(stem), prop, 1)
        setter = ("SetCsv" if csv else "SetRepeated") if is_sequence else "Set"
        return prop, f"IReadOnlyList<{value_type}>?" if is_sequence else f"{value_type}?", setter, accessor

    def add_operation(self, row, op, offset):
        op_id = row["operation_id"]
        tag = row["resource"]
        group = self.group_name(tag)
        name = CORE_OPTIONS.get(op_id, group + self.method_name(tag, op_id).removesuffix("Async") + "Options")
        if op_id == "events_list":
            self.add_event_options(name, self.parameters(op))
            return None
        excluded = {"_fields", "_cursor"} | ({"_limit", "_skip"} if offset else set())
        params = [p for p in self.parameters(op)
                  if not p.get("required") and p["name"] not in excluded and (op_id, p["name"]) not in EXCLUDED]
        return self.add_class(name, op_id, params, "CloseListOptions" if offset else "CloseRequestOptions", row["reference"])

    def add_event_options(self, name, params):
        filters = [p for p in params if p["name"] not in EVENT_OPTIONS and p["name"] != "_fields"]
        if not any(p["name"] == "request_id" for p in filters):
            filters.append({"name": "request_id", "schema": {"type": "string"}})
        self.add_class(name, "events_list", filters, None, None)

    def add_class(self, name, op_id, params, base, reference):
        props, setters = [], []
        used = set()
        for param in params:
            field = self.field(op_id, param, name)
            if field is None:
                continue
            prop, value_type, setter, accessor = field
            if prop in used:
                # Alias filters such as _type and _type__in still need distinct public names.
                rename = prop + ("In" if param["name"].endswith("__in") else "Filter")
                accessor = re.sub(rf"\b{prop}\b", rename, accessor)
                prop = rename
            if prop in used:
                raise ValueError(f"Conflicting filter {name}.{prop}")
            used.add(prop)
            wire = param["name"]
            props.append(f'    /// <summary>The Close <c>{html.escape(wire)}</c> filter.</summary>\n'
                         f'    public {value_type} {prop} {{ get; init; }}')
            setters.append(f'        query.{setter}({self.literal(wire)}, {accessor});')
        if not props:
            return None
        if reference:
            header = (f'/// <summary>Typed query options for this <see href="{reference}">Close operation</see>.</summary>\n'
                      f'public sealed class {name} : {base}')
        else:
            header = f'public sealed partial class {name}'
        self.classes[name] = (header + '\n{\n' + '\n\n'.join(props) +
                              '\n\n    internal override void ApplyFilters(CloseQueryParameters query)\n    {\n'
                              + '\n'.join(setters) + '\n    }\n}')
        return name

    def render(self, header):
        enums, formatters = [], []
        for name, members in sorted(self.enums.items()):
            values = '\n\n'.join(f'    /// <summary>The Close <c>{html.escape(wire)}</c> value.</summary>\n    {member},'
                                  for member, wire in members.items())
            enums.append(f'/// <summary>Accepted values for the Close {name} filter.</summary>\n'
                         f'public enum {name}\n{{\n{values}\n}}')
            arms = '\n'.join(f'        {name}.{member} => {self.literal(wire)},' for member, wire in members.items())
            formatters.append(f'    internal static string Format({name} value) => value switch\n    {{\n{arms}\n'
                              '        _ => throw new ArgumentOutOfRangeException(nameof(value))\n    };')
        members = [*enums, *(self.classes[name] for name in sorted(self.classes))]
        members.append('internal static class CloseQueryEnums\n{\n' + '\n\n'.join(formatters) + '\n}')
        return header + '\nnamespace Close.DotNet;\n\n' + '\n\n'.join(members) + '\n'
