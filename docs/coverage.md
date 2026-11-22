# Endpoint coverage

34 of 308 documented REST operations have named methods. Use `CloseClient.SendAsync` for rows marked **Raw JSON**.

Routes are relative to `https://api.close.com/api/v1`. OAuth and webhook signature helpers are documented separately.

| Resource | Method | Route | .NET method |
| --- | --- | --- | --- |
| activities | GET | [`/activity/`](https://developer.close.com/api/resources/activities/list) | **Raw JSON** |
| activities.calls | DELETE | [`/activity/call/{id}/`](https://developer.close.com/api/resources/activities/calls/delete) | **Raw JSON** |
| activities.calls | GET | [`/activity/call/`](https://developer.close.com/api/resources/activities/calls/list) | **Raw JSON** |
| activities.calls | GET | [`/activity/call/{id}/`](https://developer.close.com/api/resources/activities/calls/get) | **Raw JSON** |
| activities.calls | POST | [`/activity/call/`](https://developer.close.com/api/resources/activities/calls/create) | **Raw JSON** |
| activities.calls | PUT | [`/activity/call/{id}/`](https://developer.close.com/api/resources/activities/calls/update) | **Raw JSON** |
| activities.creations | GET | [`/activity/created/`](https://developer.close.com/api/resources/activities/creations/list) | **Raw JSON** |
| activities.creations | GET | [`/activity/created/{id}/`](https://developer.close.com/api/resources/activities/creations/get) | **Raw JSON** |
| activities.custom_activities | DELETE | [`/activity/custom/{id}/`](https://developer.close.com/api/resources/activities/custom-activities/delete) | **Raw JSON** |
| activities.custom_activities | GET | [`/activity/custom/`](https://developer.close.com/api/resources/activities/custom-activities/list) | **Raw JSON** |
| activities.custom_activities | GET | [`/activity/custom/{id}/`](https://developer.close.com/api/resources/activities/custom-activities/get) | **Raw JSON** |
| activities.custom_activities | POST | [`/activity/custom/`](https://developer.close.com/api/resources/activities/custom-activities/create) | **Raw JSON** |
| activities.custom_activities | PUT | [`/activity/custom/{id}/`](https://developer.close.com/api/resources/activities/custom-activities/update) | **Raw JSON** |
| activities.email_threads | DELETE | [`/activity/emailthread/{id}/`](https://developer.close.com/api/resources/activities/email-threads/delete) | **Raw JSON** |
| activities.email_threads | GET | [`/activity/emailthread/`](https://developer.close.com/api/resources/activities/email-threads/list) | **Raw JSON** |
| activities.email_threads | GET | [`/activity/emailthread/{id}/`](https://developer.close.com/api/resources/activities/email-threads/get) | **Raw JSON** |
| activities.emails | DELETE | [`/activity/email/{id}/`](https://developer.close.com/api/resources/activities/emails/delete) | **Raw JSON** |
| activities.emails | GET | [`/activity/email/`](https://developer.close.com/api/resources/activities/emails/list) | **Raw JSON** |
| activities.emails | GET | [`/activity/email/{id}/`](https://developer.close.com/api/resources/activities/emails/get) | **Raw JSON** |
| activities.emails | POST | [`/activity/email/`](https://developer.close.com/api/resources/activities/emails/create) | **Raw JSON** |
| activities.emails | PUT | [`/activity/email/{id}/`](https://developer.close.com/api/resources/activities/emails/update) | **Raw JSON** |
| activities.form_submissions | DELETE | [`/activity/form_submission/{id}/`](https://developer.close.com/api/resources/activities/form-submissions/delete) | **Raw JSON** |
| activities.form_submissions | GET | [`/activity/form_submission/`](https://developer.close.com/api/resources/activities/form-submissions/list) | **Raw JSON** |
| activities.form_submissions | GET | [`/activity/form_submission/{id}/`](https://developer.close.com/api/resources/activities/form-submissions/get) | **Raw JSON** |
| activities.import_updates | DELETE | [`/activity/import_update/{id}/`](https://developer.close.com/api/resources/activities/import-updates/delete) | **Raw JSON** |
| activities.import_updates | GET | [`/activity/import_update/`](https://developer.close.com/api/resources/activities/import-updates/list) | **Raw JSON** |
| activities.import_updates | GET | [`/activity/import_update/{id}/`](https://developer.close.com/api/resources/activities/import-updates/get) | **Raw JSON** |
| activities.lead_merges | GET | [`/activity/lead_merge/`](https://developer.close.com/api/resources/activities/lead-merges/list) | **Raw JSON** |
| activities.lead_merges | GET | [`/activity/lead_merge/{id}/`](https://developer.close.com/api/resources/activities/lead-merges/get) | **Raw JSON** |
| activities.lead_status_changes | DELETE | [`/activity/status_change/lead/{id}/`](https://developer.close.com/api/resources/activities/lead-status-changes/delete) | **Raw JSON** |
| activities.lead_status_changes | GET | [`/activity/status_change/lead/`](https://developer.close.com/api/resources/activities/lead-status-changes/list) | **Raw JSON** |
| activities.lead_status_changes | GET | [`/activity/status_change/lead/{id}/`](https://developer.close.com/api/resources/activities/lead-status-changes/get) | **Raw JSON** |
| activities.lead_status_changes | POST | [`/activity/status_change/lead/`](https://developer.close.com/api/resources/activities/lead-status-changes/create) | **Raw JSON** |
| activities.meetings | DELETE | [`/activity/meeting/{id}/`](https://developer.close.com/api/resources/activities/meetings/delete) | **Raw JSON** |
| activities.meetings | GET | [`/activity/meeting/`](https://developer.close.com/api/resources/activities/meetings/list) | **Raw JSON** |
| activities.meetings | GET | [`/activity/meeting/{id}/`](https://developer.close.com/api/resources/activities/meetings/get) | **Raw JSON** |
| activities.meetings | POST | [`/activity/meeting/{id}/integration/`](https://developer.close.com/api/resources/activities/meetings/create-integration) | **Raw JSON** |
| activities.meetings | PUT | [`/activity/meeting/{id}/`](https://developer.close.com/api/resources/activities/meetings/update) | **Raw JSON** |
| activities.notes | DELETE | [`/activity/note/{id}/`](https://developer.close.com/api/resources/activities/notes/delete) | `Notes.DeleteAsync` |
| activities.notes | GET | [`/activity/note/`](https://developer.close.com/api/resources/activities/notes/list) | `Notes.ListPageAsync` |
| activities.notes | GET | [`/activity/note/{id}/`](https://developer.close.com/api/resources/activities/notes/get) | `Notes.GetAsync` |
| activities.notes | POST | [`/activity/note/`](https://developer.close.com/api/resources/activities/notes/create) | `Notes.CreateAsync` |
| activities.notes | PUT | [`/activity/note/{id}/`](https://developer.close.com/api/resources/activities/notes/update) | `Notes.UpdateAsync` |
| activities.opportunity_status_changes | DELETE | [`/activity/status_change/opportunity/{id}/`](https://developer.close.com/api/resources/activities/opportunity-status-changes/delete) | **Raw JSON** |
| activities.opportunity_status_changes | GET | [`/activity/status_change/opportunity/`](https://developer.close.com/api/resources/activities/opportunity-status-changes/list) | **Raw JSON** |
| activities.opportunity_status_changes | GET | [`/activity/status_change/opportunity/{id}/`](https://developer.close.com/api/resources/activities/opportunity-status-changes/get) | **Raw JSON** |
| activities.opportunity_status_changes | POST | [`/activity/status_change/opportunity/`](https://developer.close.com/api/resources/activities/opportunity-status-changes/create) | **Raw JSON** |
| activities.sms | DELETE | [`/activity/sms/{id}/`](https://developer.close.com/api/resources/activities/sms/delete) | **Raw JSON** |
| activities.sms | GET | [`/activity/sms/`](https://developer.close.com/api/resources/activities/sms/list) | **Raw JSON** |
| activities.sms | GET | [`/activity/sms/{id}/`](https://developer.close.com/api/resources/activities/sms/get) | **Raw JSON** |
| activities.sms | POST | [`/activity/sms/`](https://developer.close.com/api/resources/activities/sms/create) | **Raw JSON** |
| activities.sms | PUT | [`/activity/sms/{id}/`](https://developer.close.com/api/resources/activities/sms/update) | **Raw JSON** |
| activities.task_completions | DELETE | [`/activity/task_completed/{id}/`](https://developer.close.com/api/resources/activities/task-completions/delete) | **Raw JSON** |
| activities.task_completions | GET | [`/activity/task_completed/`](https://developer.close.com/api/resources/activities/task-completions/list) | **Raw JSON** |
| activities.task_completions | GET | [`/activity/task_completed/{id}/`](https://developer.close.com/api/resources/activities/task-completions/get) | **Raw JSON** |
| activities.whatsapp_messages | DELETE | [`/activity/whatsapp_message/{id}/`](https://developer.close.com/api/resources/activities/whatsapp/delete) | **Raw JSON** |
| activities.whatsapp_messages | GET | [`/activity/whatsapp_message/`](https://developer.close.com/api/resources/activities/whatsapp/list) | **Raw JSON** |
| activities.whatsapp_messages | GET | [`/activity/whatsapp_message/{id}/`](https://developer.close.com/api/resources/activities/whatsapp/get) | **Raw JSON** |
| activities.whatsapp_messages | POST | [`/activity/whatsapp_message/`](https://developer.close.com/api/resources/activities/whatsapp/create) | **Raw JSON** |
| activities.whatsapp_messages | PUT | [`/activity/whatsapp_message/{id}/`](https://developer.close.com/api/resources/activities/whatsapp/update) | **Raw JSON** |
| advanced_filtering | POST | [`/data/search/`](https://developer.close.com/api/resources/advanced-filtering/search) | `Search.SearchPageAsync` |
| blocked_phone_numbers | DELETE | [`/blocked_phone_number/{id}/`](https://developer.close.com/api/resources/blocked-phone-numbers/delete) | **Raw JSON** |
| blocked_phone_numbers | GET | [`/blocked_phone_number/`](https://developer.close.com/api/resources/blocked-phone-numbers/list) | **Raw JSON** |
| blocked_phone_numbers | GET | [`/blocked_phone_number/settings/`](https://developer.close.com/api/resources/blocked-phone-numbers/get-settings) | **Raw JSON** |
| blocked_phone_numbers | GET | [`/blocked_phone_number/{id}/`](https://developer.close.com/api/resources/blocked-phone-numbers/get) | **Raw JSON** |
| blocked_phone_numbers | POST | [`/blocked_phone_number/`](https://developer.close.com/api/resources/blocked-phone-numbers/create) | **Raw JSON** |
| blocked_phone_numbers | PUT | [`/blocked_phone_number/settings/`](https://developer.close.com/api/resources/blocked-phone-numbers/update-settings) | **Raw JSON** |
| blocked_phone_numbers | PUT | [`/blocked_phone_number/{id}/`](https://developer.close.com/api/resources/blocked-phone-numbers/update) | **Raw JSON** |
| bulk_actions.delete | GET | [`/bulk_action/delete/`](https://developer.close.com/api/resources/bulk-actions/delete/list) | **Raw JSON** |
| bulk_actions.delete | GET | [`/bulk_action/delete/{id}/`](https://developer.close.com/api/resources/bulk-actions/delete/get) | **Raw JSON** |
| bulk_actions.delete | POST | [`/bulk_action/delete/`](https://developer.close.com/api/resources/bulk-actions/delete/create) | **Raw JSON** |
| bulk_actions.edit | GET | [`/bulk_action/edit/`](https://developer.close.com/api/resources/bulk-actions/edit/list) | **Raw JSON** |
| bulk_actions.edit | GET | [`/bulk_action/edit/{id}/`](https://developer.close.com/api/resources/bulk-actions/edit/get) | **Raw JSON** |
| bulk_actions.edit | POST | [`/bulk_action/edit/`](https://developer.close.com/api/resources/bulk-actions/edit/create) | **Raw JSON** |
| bulk_actions.email | GET | [`/bulk_action/email/`](https://developer.close.com/api/resources/bulk-actions/email/list) | **Raw JSON** |
| bulk_actions.email | GET | [`/bulk_action/email/{id}/`](https://developer.close.com/api/resources/bulk-actions/email/get) | **Raw JSON** |
| bulk_actions.email | POST | [`/bulk_action/email/`](https://developer.close.com/api/resources/bulk-actions/email/create) | **Raw JSON** |
| bulk_actions.sequence_subscriptions | GET | [`/bulk_action/sequence_subscription/`](https://developer.close.com/api/resources/bulk-actions/sequence-subscriptions/list) | **Raw JSON** |
| bulk_actions.sequence_subscriptions | GET | [`/bulk_action/sequence_subscription/{id}/`](https://developer.close.com/api/resources/bulk-actions/sequence-subscriptions/get) | **Raw JSON** |
| bulk_actions.sequence_subscriptions | POST | [`/bulk_action/sequence_subscription/`](https://developer.close.com/api/resources/bulk-actions/sequence-subscriptions/create) | **Raw JSON** |
| comments | DELETE | [`/comment/{id}/`](https://developer.close.com/api/resources/comments/delete) | **Raw JSON** |
| comments | GET | [`/comment/`](https://developer.close.com/api/resources/comments/list) | **Raw JSON** |
| comments | GET | [`/comment/{id}/`](https://developer.close.com/api/resources/comments/get) | **Raw JSON** |
| comments | GET | [`/comment_thread/`](https://developer.close.com/api/resources/comments/list-threads) | **Raw JSON** |
| comments | GET | [`/comment_thread/{id}/`](https://developer.close.com/api/resources/comments/get-thread) | **Raw JSON** |
| comments | POST | [`/comment/`](https://developer.close.com/api/resources/comments/create) | **Raw JSON** |
| comments | PUT | [`/comment/{id}/`](https://developer.close.com/api/resources/comments/update) | **Raw JSON** |
| connected_accounts | GET | [`/connected_account/`](https://developer.close.com/api/resources/connected-accounts/list) | **Raw JSON** |
| connected_accounts | GET | [`/connected_account/{id}/`](https://developer.close.com/api/resources/connected-accounts/get) | **Raw JSON** |
| contacts | DELETE | [`/contact/{id}/`](https://developer.close.com/api/resources/contacts/delete) | `Contacts.DeleteAsync` |
| contacts | GET | [`/contact/`](https://developer.close.com/api/resources/contacts/list) | `Contacts.ListPageAsync` |
| contacts | GET | [`/contact/{id}/`](https://developer.close.com/api/resources/contacts/get) | `Contacts.GetAsync` |
| contacts | POST | [`/contact/`](https://developer.close.com/api/resources/contacts/create) | `Contacts.CreateAsync` |
| contacts | PUT | [`/contact/{id}/`](https://developer.close.com/api/resources/contacts/update) | `Contacts.UpdateAsync` |
| custom_activity_types | DELETE | [`/custom_activity/{id}/`](https://developer.close.com/api/resources/custom-activity-types/delete) | **Raw JSON** |
| custom_activity_types | GET | [`/custom_activity/`](https://developer.close.com/api/resources/custom-activity-types/list) | **Raw JSON** |
| custom_activity_types | GET | [`/custom_activity/{id}/`](https://developer.close.com/api/resources/custom-activity-types/get) | **Raw JSON** |
| custom_activity_types | POST | [`/custom_activity/`](https://developer.close.com/api/resources/custom-activity-types/create) | **Raw JSON** |
| custom_activity_types | PUT | [`/custom_activity/{id}/`](https://developer.close.com/api/resources/custom-activity-types/update) | **Raw JSON** |
| custom_field_schemas | GET | [`/custom_field_schema/{object_type}/`](https://developer.close.com/api/resources/custom-fields/custom-field-schemas/get) | **Raw JSON** |
| custom_field_schemas | PUT | [`/custom_field_schema/{object_type}/`](https://developer.close.com/api/resources/custom-fields/custom-field-schemas/update) | **Raw JSON** |
| custom_fields.activity | DELETE | [`/custom_field/activity/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-activity/delete) | **Raw JSON** |
| custom_fields.activity | GET | [`/custom_field/activity/`](https://developer.close.com/api/resources/custom-fields/custom-fields-activity/list) | **Raw JSON** |
| custom_fields.activity | GET | [`/custom_field/activity/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-activity/get) | **Raw JSON** |
| custom_fields.activity | POST | [`/custom_field/activity/`](https://developer.close.com/api/resources/custom-fields/custom-fields-activity/create) | **Raw JSON** |
| custom_fields.activity | PUT | [`/custom_field/activity/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-activity/update) | **Raw JSON** |
| custom_fields.contact | DELETE | [`/custom_field/contact/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-contact/delete) | **Raw JSON** |
| custom_fields.contact | GET | [`/custom_field/contact/`](https://developer.close.com/api/resources/custom-fields/custom-fields-contact/list) | **Raw JSON** |
| custom_fields.contact | GET | [`/custom_field/contact/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-contact/get) | **Raw JSON** |
| custom_fields.contact | POST | [`/custom_field/contact/`](https://developer.close.com/api/resources/custom-fields/custom-fields-contact/create) | **Raw JSON** |
| custom_fields.contact | PUT | [`/custom_field/contact/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-contact/update) | **Raw JSON** |
| custom_fields.custom_object | DELETE | [`/custom_field/custom_object_type/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-custom-object/delete) | **Raw JSON** |
| custom_fields.custom_object | GET | [`/custom_field/custom_object_type/`](https://developer.close.com/api/resources/custom-fields/custom-fields-custom-object/list) | **Raw JSON** |
| custom_fields.custom_object | GET | [`/custom_field/custom_object_type/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-custom-object/get) | **Raw JSON** |
| custom_fields.custom_object | POST | [`/custom_field/custom_object_type/`](https://developer.close.com/api/resources/custom-fields/custom-fields-custom-object/create) | **Raw JSON** |
| custom_fields.custom_object | PUT | [`/custom_field/custom_object_type/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-custom-object/update) | **Raw JSON** |
| custom_fields.lead | DELETE | [`/custom_field/lead/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-lead/delete) | **Raw JSON** |
| custom_fields.lead | GET | [`/custom_field/lead/`](https://developer.close.com/api/resources/custom-fields/custom-fields-lead/list) | **Raw JSON** |
| custom_fields.lead | GET | [`/custom_field/lead/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-lead/get) | **Raw JSON** |
| custom_fields.lead | POST | [`/custom_field/lead/`](https://developer.close.com/api/resources/custom-fields/custom-fields-lead/create) | **Raw JSON** |
| custom_fields.lead | PUT | [`/custom_field/lead/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-lead/update) | **Raw JSON** |
| custom_fields.opportunity | DELETE | [`/custom_field/opportunity/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-opportunity/delete) | **Raw JSON** |
| custom_fields.opportunity | GET | [`/custom_field/opportunity/`](https://developer.close.com/api/resources/custom-fields/custom-fields-opportunity/list) | **Raw JSON** |
| custom_fields.opportunity | GET | [`/custom_field/opportunity/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-opportunity/get) | **Raw JSON** |
| custom_fields.opportunity | POST | [`/custom_field/opportunity/`](https://developer.close.com/api/resources/custom-fields/custom-fields-opportunity/create) | **Raw JSON** |
| custom_fields.opportunity | PUT | [`/custom_field/opportunity/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-opportunity/update) | **Raw JSON** |
| custom_fields.shared | DELETE | [`/custom_field/shared/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-shared/delete) | **Raw JSON** |
| custom_fields.shared | DELETE | [`/custom_field/shared/{scf_id}/association/{object_type}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-shared/delete-association) | **Raw JSON** |
| custom_fields.shared | GET | [`/custom_field/shared/`](https://developer.close.com/api/resources/custom-fields/custom-fields-shared/list) | **Raw JSON** |
| custom_fields.shared | GET | [`/custom_field/shared/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-shared/get) | **Raw JSON** |
| custom_fields.shared | GET | [`/custom_field/shared/{scf_id}/association/{object_type}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-shared/get-association) | **Raw JSON** |
| custom_fields.shared | POST | [`/custom_field/shared/`](https://developer.close.com/api/resources/custom-fields/custom-fields-shared/create) | **Raw JSON** |
| custom_fields.shared | POST | [`/custom_field/shared/{scf_id}/association/`](https://developer.close.com/api/resources/custom-fields/custom-fields-shared/create-association) | **Raw JSON** |
| custom_fields.shared | PUT | [`/custom_field/shared/{id}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-shared/update) | **Raw JSON** |
| custom_fields.shared | PUT | [`/custom_field/shared/{scf_id}/association/{object_type}/`](https://developer.close.com/api/resources/custom-fields/custom-fields-shared/update-association) | **Raw JSON** |
| custom_object_types | DELETE | [`/custom_object_type/{id}/`](https://developer.close.com/api/resources/custom-object-types/delete) | **Raw JSON** |
| custom_object_types | GET | [`/custom_object_type/`](https://developer.close.com/api/resources/custom-object-types/list) | **Raw JSON** |
| custom_object_types | GET | [`/custom_object_type/{id}/`](https://developer.close.com/api/resources/custom-object-types/get) | **Raw JSON** |
| custom_object_types | POST | [`/custom_object_type/`](https://developer.close.com/api/resources/custom-object-types/create) | **Raw JSON** |
| custom_object_types | PUT | [`/custom_object_type/{id}/`](https://developer.close.com/api/resources/custom-object-types/update) | **Raw JSON** |
| custom_objects | DELETE | [`/custom_object/{id}/`](https://developer.close.com/api/resources/custom-objects/delete) | **Raw JSON** |
| custom_objects | GET | [`/custom_object/`](https://developer.close.com/api/resources/custom-objects/list) | **Raw JSON** |
| custom_objects | GET | [`/custom_object/{id}/`](https://developer.close.com/api/resources/custom-objects/get) | **Raw JSON** |
| custom_objects | POST | [`/custom_object/`](https://developer.close.com/api/resources/custom-objects/create) | **Raw JSON** |
| custom_objects | PUT | [`/custom_object/{id}/`](https://developer.close.com/api/resources/custom-objects/update) | **Raw JSON** |
| dialers | GET | [`/dialer/`](https://developer.close.com/api/resources/dialers/list) | **Raw JSON** |
| dialers | GET | [`/dialer/{id}/`](https://developer.close.com/api/resources/dialers/get) | **Raw JSON** |
| email_templates | DELETE | [`/email_template/{id}/`](https://developer.close.com/api/resources/email-templates/delete) | **Raw JSON** |
| email_templates | GET | [`/email_template/`](https://developer.close.com/api/resources/email-templates/list) | **Raw JSON** |
| email_templates | GET | [`/email_template/{id}/`](https://developer.close.com/api/resources/email-templates/get) | **Raw JSON** |
| email_templates | GET | [`/email_template/{id}/render/`](https://developer.close.com/api/resources/email-templates/render) | **Raw JSON** |
| email_templates | POST | [`/email_template/`](https://developer.close.com/api/resources/email-templates/create) | **Raw JSON** |
| email_templates | PUT | [`/email_template/{id}/`](https://developer.close.com/api/resources/email-templates/update) | **Raw JSON** |
| events | GET | [`/event/`](https://developer.close.com/api/resources/events/list) | `Events.ListPageAsync` |
| events | GET | [`/event/{id}/`](https://developer.close.com/api/resources/events/get) | `Events.GetAsync` |
| exports | GET | [`/export/`](https://developer.close.com/api/resources/exports/list) | **Raw JSON** |
| exports | GET | [`/export/lead/`](https://developer.close.com/api/resources/exports/list-lead) | **Raw JSON** |
| exports | GET | [`/export/lead/{id}/`](https://developer.close.com/api/resources/exports/get-lead) | **Raw JSON** |
| exports | GET | [`/export/opportunity/`](https://developer.close.com/api/resources/exports/list-opportunity) | **Raw JSON** |
| exports | GET | [`/export/opportunity/{id}/`](https://developer.close.com/api/resources/exports/get-opportunity) | **Raw JSON** |
| exports | GET | [`/export/{id}/`](https://developer.close.com/api/resources/exports/get) | **Raw JSON** |
| exports | POST | [`/export/lead/`](https://developer.close.com/api/resources/exports/create-lead) | **Raw JSON** |
| exports | POST | [`/export/opportunity/`](https://developer.close.com/api/resources/exports/create-opportunity) | **Raw JSON** |
| field_enrichment | POST | [`/enrich_field/`](https://developer.close.com/api/resources/field-enrichment/create) | **Raw JSON** |
| files | POST | [`/files/upload/`](https://developer.close.com/api/resources/files/create) | **Raw JSON** |
| forms | GET | [`/form/`](https://developer.close.com/api/resources/forms/list) | **Raw JSON** |
| forms | GET | [`/form/{id}/`](https://developer.close.com/api/resources/forms/fetch) | **Raw JSON** |
| groups | DELETE | [`/group/{group_id}/member/{user_id}/`](https://developer.close.com/api/resources/groups/remove-member) | **Raw JSON** |
| groups | DELETE | [`/group/{id}/`](https://developer.close.com/api/resources/groups/delete) | **Raw JSON** |
| groups | GET | [`/group/`](https://developer.close.com/api/resources/groups/list) | **Raw JSON** |
| groups | GET | [`/group/{id}/`](https://developer.close.com/api/resources/groups/get) | **Raw JSON** |
| groups | POST | [`/group/`](https://developer.close.com/api/resources/groups/create) | **Raw JSON** |
| groups | POST | [`/group/{group_id}/member/`](https://developer.close.com/api/resources/groups/add-member) | **Raw JSON** |
| groups | PUT | [`/group/{id}/`](https://developer.close.com/api/resources/groups/update) | **Raw JSON** |
| integration_links | DELETE | [`/integration_link/{id}/`](https://developer.close.com/api/resources/integration-links/delete) | **Raw JSON** |
| integration_links | GET | [`/integration_link/`](https://developer.close.com/api/resources/integration-links/list) | **Raw JSON** |
| integration_links | GET | [`/integration_link/{id}/`](https://developer.close.com/api/resources/integration-links/get) | **Raw JSON** |
| integration_links | POST | [`/integration_link/`](https://developer.close.com/api/resources/integration-links/create) | **Raw JSON** |
| integration_links | PUT | [`/integration_link/{id}/`](https://developer.close.com/api/resources/integration-links/update) | **Raw JSON** |
| lead_statuses | DELETE | [`/status/lead/{id}/`](https://developer.close.com/api/resources/lead-statuses/delete) | **Raw JSON** |
| lead_statuses | GET | [`/status/lead/`](https://developer.close.com/api/resources/lead-statuses/list) | **Raw JSON** |
| lead_statuses | GET | [`/status/lead/{id}/`](https://developer.close.com/api/resources/lead-statuses/get) | **Raw JSON** |
| lead_statuses | POST | [`/status/lead/`](https://developer.close.com/api/resources/lead-statuses/create) | **Raw JSON** |
| lead_statuses | PUT | [`/status/lead/{id}/`](https://developer.close.com/api/resources/lead-statuses/update) | **Raw JSON** |
| leads | DELETE | [`/lead/{id}/`](https://developer.close.com/api/resources/leads/delete) | `Leads.DeleteAsync` |
| leads | GET | [`/lead/`](https://developer.close.com/api/resources/leads/list) | `Leads.ListPageAsync` |
| leads | GET | [`/lead/{id}/`](https://developer.close.com/api/resources/leads/get) | `Leads.GetAsync` |
| leads | GET | [`/lead/{id}/files/`](https://developer.close.com/api/resources/leads/files-list) | **Raw JSON** |
| leads | POST | [`/lead/`](https://developer.close.com/api/resources/leads/create) | `Leads.CreateAsync` |
| leads | POST | [`/lead/merge/`](https://developer.close.com/api/resources/leads/merge) | **Raw JSON** |
| leads | POST | [`/lead/{id}/files/`](https://developer.close.com/api/resources/leads/files-create) | **Raw JSON** |
| leads | PUT | [`/lead/{id}/`](https://developer.close.com/api/resources/leads/update) | `Leads.UpdateAsync` |
| memberships | DELETE | [`/membership/{id}/`](https://developer.close.com/api/resources/memberships/delete) | **Raw JSON** |
| memberships | GET | [`/membership/{id}/pinned_views/`](https://developer.close.com/api/resources/memberships/get-pinned-views) | **Raw JSON** |
| memberships | POST | [`/membership/`](https://developer.close.com/api/resources/memberships/create) | **Raw JSON** |
| memberships | PUT | [`/membership/`](https://developer.close.com/api/resources/memberships/bulk-update) | **Raw JSON** |
| memberships | PUT | [`/membership/{id}/`](https://developer.close.com/api/resources/memberships/update) | **Raw JSON** |
| memberships | PUT | [`/membership/{id}/pinned_views/`](https://developer.close.com/api/resources/memberships/update-pinned-views) | **Raw JSON** |
| opportunities | DELETE | [`/opportunity/{id}/`](https://developer.close.com/api/resources/opportunities/delete) | `Opportunities.DeleteAsync` |
| opportunities | GET | [`/opportunity/`](https://developer.close.com/api/resources/opportunities/list) | `Opportunities.ListPageAsync` |
| opportunities | GET | [`/opportunity/{id}/`](https://developer.close.com/api/resources/opportunities/get) | `Opportunities.GetAsync` |
| opportunities | POST | [`/opportunity/`](https://developer.close.com/api/resources/opportunities/create) | `Opportunities.CreateAsync` |
| opportunities | PUT | [`/opportunity/{id}/`](https://developer.close.com/api/resources/opportunities/update) | `Opportunities.UpdateAsync` |
| opportunity_statuses | DELETE | [`/status/opportunity/{id}/`](https://developer.close.com/api/resources/opportunity-statuses/delete) | **Raw JSON** |
| opportunity_statuses | GET | [`/status/opportunity/`](https://developer.close.com/api/resources/opportunity-statuses/list) | **Raw JSON** |
| opportunity_statuses | GET | [`/status/opportunity/{id}/`](https://developer.close.com/api/resources/opportunity-statuses/get) | **Raw JSON** |
| opportunity_statuses | POST | [`/status/opportunity/`](https://developer.close.com/api/resources/opportunity-statuses/create) | **Raw JSON** |
| opportunity_statuses | PUT | [`/status/opportunity/{id}/`](https://developer.close.com/api/resources/opportunity-statuses/update) | **Raw JSON** |
| organizations | GET | [`/organization/{id}/`](https://developer.close.com/api/resources/organizations/get) | **Raw JSON** |
| organizations | PUT | [`/organization/{id}/`](https://developer.close.com/api/resources/organizations/update) | **Raw JSON** |
| outcomes | DELETE | [`/outcome/{id}/`](https://developer.close.com/api/resources/outcomes/delete) | **Raw JSON** |
| outcomes | GET | [`/outcome/`](https://developer.close.com/api/resources/outcomes/list) | **Raw JSON** |
| outcomes | GET | [`/outcome/{id}/`](https://developer.close.com/api/resources/outcomes/get) | **Raw JSON** |
| outcomes | POST | [`/outcome/`](https://developer.close.com/api/resources/outcomes/create) | **Raw JSON** |
| outcomes | PUT | [`/outcome/{id}/`](https://developer.close.com/api/resources/outcomes/update) | **Raw JSON** |
| phone_numbers | DELETE | [`/phone_number/{id}/`](https://developer.close.com/api/resources/phone-numbers/delete) | **Raw JSON** |
| phone_numbers | GET | [`/phone_number/`](https://developer.close.com/api/resources/phone-numbers/list) | **Raw JSON** |
| phone_numbers | GET | [`/phone_number/{id}/`](https://developer.close.com/api/resources/phone-numbers/get) | **Raw JSON** |
| phone_numbers | POST | [`/phone_number/request/internal/`](https://developer.close.com/api/resources/phone-numbers/create) | **Raw JSON** |
| phone_numbers | PUT | [`/phone_number/{id}/`](https://developer.close.com/api/resources/phone-numbers/update) | **Raw JSON** |
| pipelines | DELETE | [`/pipeline/{id}/`](https://developer.close.com/api/resources/pipelines/delete) | **Raw JSON** |
| pipelines | GET | [`/pipeline/`](https://developer.close.com/api/resources/pipelines/list) | **Raw JSON** |
| pipelines | GET | [`/pipeline/{id}/`](https://developer.close.com/api/resources/pipelines/get) | **Raw JSON** |
| pipelines | POST | [`/pipeline/`](https://developer.close.com/api/resources/pipelines/create) | **Raw JSON** |
| pipelines | PUT | [`/pipeline/{id}/`](https://developer.close.com/api/resources/pipelines/update) | **Raw JSON** |
| playbooks | DELETE | [`/playbook/{id}/`](https://developer.close.com/api/resources/playbooks/delete) | **Raw JSON** |
| playbooks | GET | [`/playbook/`](https://developer.close.com/api/resources/playbooks/list) | **Raw JSON** |
| playbooks | GET | [`/playbook/{id}/`](https://developer.close.com/api/resources/playbooks/get) | **Raw JSON** |
| playbooks | POST | [`/playbook/`](https://developer.close.com/api/resources/playbooks/create) | **Raw JSON** |
| playbooks | POST | [`/playbook/{id}/archive/`](https://developer.close.com/api/resources/playbooks/archive) | **Raw JSON** |
| playbooks | POST | [`/playbook/{id}/unarchive/`](https://developer.close.com/api/resources/playbooks/unarchive) | **Raw JSON** |
| playbooks | PUT | [`/playbook/{id}/`](https://developer.close.com/api/resources/playbooks/update) | **Raw JSON** |
| reporting | GET | [`/report/activity/metrics/`](https://developer.close.com/api/resources/reporting/list-activity-metrics) | **Raw JSON** |
| reporting | GET | [`/report/custom/{org_id}/`](https://developer.close.com/api/resources/reporting/get-custom) | **Raw JSON** |
| reporting | GET | [`/report/sent_emails/{org_id}/`](https://developer.close.com/api/resources/reporting/get-sent-emails) | **Raw JSON** |
| reporting | GET | [`/report/statuses/lead/{org_id}/`](https://developer.close.com/api/resources/reporting/get-lead-statuses) | **Raw JSON** |
| reporting | GET | [`/report/statuses/opportunity/{org_id}/`](https://developer.close.com/api/resources/reporting/get-opportunity-statuses) | **Raw JSON** |
| reporting | POST | [`/report/activity/`](https://developer.close.com/api/resources/reporting/get-activity) | **Raw JSON** |
| reporting | POST | [`/report/funnel/opportunity/stages/`](https://developer.close.com/api/resources/reporting/get-funnel-stages) | **Raw JSON** |
| reporting | POST | [`/report/funnel/opportunity/totals/`](https://developer.close.com/api/resources/reporting/get-funnel-totals) | **Raw JSON** |
| roles | DELETE | [`/role/{id}/`](https://developer.close.com/api/resources/roles/delete) | **Raw JSON** |
| roles | GET | [`/role/`](https://developer.close.com/api/resources/roles/list) | **Raw JSON** |
| roles | GET | [`/role/{id}/`](https://developer.close.com/api/resources/roles/get) | **Raw JSON** |
| roles | POST | [`/role/`](https://developer.close.com/api/resources/roles/create) | **Raw JSON** |
| roles | PUT | [`/role/{id}/`](https://developer.close.com/api/resources/roles/update) | **Raw JSON** |
| scheduling_links | DELETE | [`/scheduling_link/integration/{source_id}/`](https://developer.close.com/api/resources/scheduling-links/delete-integration) | **Raw JSON** |
| scheduling_links | DELETE | [`/scheduling_link/{id}/`](https://developer.close.com/api/resources/scheduling-links/delete) | **Raw JSON** |
| scheduling_links | DELETE | [`/shared_scheduling_link/{id}/`](https://developer.close.com/api/resources/scheduling-links/delete-shared) | **Raw JSON** |
| scheduling_links | GET | [`/scheduling_link/`](https://developer.close.com/api/resources/scheduling-links/list) | **Raw JSON** |
| scheduling_links | GET | [`/scheduling_link/{id}/`](https://developer.close.com/api/resources/scheduling-links/get) | **Raw JSON** |
| scheduling_links | GET | [`/shared_scheduling_link/`](https://developer.close.com/api/resources/scheduling-links/list-shared) | **Raw JSON** |
| scheduling_links | GET | [`/shared_scheduling_link/{id}/`](https://developer.close.com/api/resources/scheduling-links/get-shared) | **Raw JSON** |
| scheduling_links | POST | [`/scheduling_link/`](https://developer.close.com/api/resources/scheduling-links/create) | **Raw JSON** |
| scheduling_links | POST | [`/scheduling_link/integration/`](https://developer.close.com/api/resources/scheduling-links/create-integration) | **Raw JSON** |
| scheduling_links | POST | [`/shared_scheduling_link/`](https://developer.close.com/api/resources/scheduling-links/create-shared) | **Raw JSON** |
| scheduling_links | POST | [`/shared_scheduling_link_association/`](https://developer.close.com/api/resources/scheduling-links/create-shared-association) | **Raw JSON** |
| scheduling_links | POST | [`/shared_scheduling_link_association/unmap/`](https://developer.close.com/api/resources/scheduling-links/delete-shared-association) | **Raw JSON** |
| scheduling_links | PUT | [`/scheduling_link/{id}/`](https://developer.close.com/api/resources/scheduling-links/update) | **Raw JSON** |
| scheduling_links | PUT | [`/shared_scheduling_link/{id}/`](https://developer.close.com/api/resources/scheduling-links/update-shared) | **Raw JSON** |
| send_as | DELETE | [`/send_as/`](https://developer.close.com/api/resources/send-as/delete-by-user) | **Raw JSON** |
| send_as | DELETE | [`/send_as/{id}/`](https://developer.close.com/api/resources/send-as/delete) | **Raw JSON** |
| send_as | GET | [`/send_as/`](https://developer.close.com/api/resources/send-as/list) | **Raw JSON** |
| send_as | GET | [`/send_as/{id}/`](https://developer.close.com/api/resources/send-as/get) | **Raw JSON** |
| send_as | POST | [`/send_as/`](https://developer.close.com/api/resources/send-as/create) | **Raw JSON** |
| send_as | POST | [`/send_as/bulk/`](https://developer.close.com/api/resources/send-as/bulk-create) | **Raw JSON** |
| sequences | DELETE | [`/sequence/{id}/`](https://developer.close.com/api/resources/sequences/delete) | **Raw JSON** |
| sequences | DELETE | [`/sequence_subscription/{id}/`](https://developer.close.com/api/resources/sequences/delete-subscription) | **Raw JSON** |
| sequences | GET | [`/sequence/`](https://developer.close.com/api/resources/sequences/list) | **Raw JSON** |
| sequences | GET | [`/sequence/{id}/`](https://developer.close.com/api/resources/sequences/get) | **Raw JSON** |
| sequences | GET | [`/sequence_subscription/`](https://developer.close.com/api/resources/sequences/list-subscriptions) | **Raw JSON** |
| sequences | GET | [`/sequence_subscription/{id}/`](https://developer.close.com/api/resources/sequences/get-subscription) | **Raw JSON** |
| sequences | POST | [`/sequence/`](https://developer.close.com/api/resources/sequences/create) | **Raw JSON** |
| sequences | POST | [`/sequence_subscription/`](https://developer.close.com/api/resources/sequences/create-subscription) | **Raw JSON** |
| sequences | PUT | [`/sequence/{id}/`](https://developer.close.com/api/resources/sequences/update) | **Raw JSON** |
| sequences | PUT | [`/sequence_subscription/{id}/`](https://developer.close.com/api/resources/sequences/update-subscription) | **Raw JSON** |
| smart_views | DELETE | [`/saved_search/{id}/`](https://developer.close.com/api/resources/smart-views/delete) | **Raw JSON** |
| smart_views | GET | [`/saved_search/`](https://developer.close.com/api/resources/smart-views/list) | **Raw JSON** |
| smart_views | GET | [`/saved_search/{id}/`](https://developer.close.com/api/resources/smart-views/get) | **Raw JSON** |
| smart_views | POST | [`/saved_search/`](https://developer.close.com/api/resources/smart-views/create) | **Raw JSON** |
| smart_views | PUT | [`/saved_search/{id}/`](https://developer.close.com/api/resources/smart-views/update) | **Raw JSON** |
| sms_templates | DELETE | [`/sms_template/{id}/`](https://developer.close.com/api/resources/sms-templates/delete) | **Raw JSON** |
| sms_templates | GET | [`/sms_template/`](https://developer.close.com/api/resources/sms-templates/list) | **Raw JSON** |
| sms_templates | GET | [`/sms_template/{id}/`](https://developer.close.com/api/resources/sms-templates/get) | **Raw JSON** |
| sms_templates | GET | [`/sms_template/{id}/render/`](https://developer.close.com/api/resources/sms-templates/render) | **Raw JSON** |
| sms_templates | POST | [`/sms_template/`](https://developer.close.com/api/resources/sms-templates/create) | **Raw JSON** |
| sms_templates | PUT | [`/sms_template/{id}/`](https://developer.close.com/api/resources/sms-templates/update) | **Raw JSON** |
| standard_object_labels | DELETE | [`/standard_object_label/{label_type}/`](https://developer.close.com/api/resources/standard-object-labels/delete) | **Raw JSON** |
| standard_object_labels | GET | [`/standard_object_label/`](https://developer.close.com/api/resources/standard-object-labels/list) | **Raw JSON** |
| standard_object_labels | GET | [`/standard_object_label/{label_type}/`](https://developer.close.com/api/resources/standard-object-labels/get) | **Raw JSON** |
| standard_object_labels | PUT | [`/standard_object_label/{label_type}/`](https://developer.close.com/api/resources/standard-object-labels/update) | **Raw JSON** |
| tasks | DELETE | [`/task/{id}/`](https://developer.close.com/api/resources/tasks/delete) | `Tasks.DeleteAsync` |
| tasks | GET | [`/task/`](https://developer.close.com/api/resources/tasks/list) | `Tasks.ListPageAsync` |
| tasks | GET | [`/task/{id}/`](https://developer.close.com/api/resources/tasks/get) | `Tasks.GetAsync` |
| tasks | POST | [`/task/`](https://developer.close.com/api/resources/tasks/create) | `Tasks.CreateAsync` |
| tasks | PUT | [`/task/`](https://developer.close.com/api/resources/tasks/bulk-update) | **Raw JSON** |
| tasks | PUT | [`/task/{id}/`](https://developer.close.com/api/resources/tasks/update) | `Tasks.UpdateAsync` |
| unsubscribed_emails | DELETE | [`/unsubscribe/email/{email_address}/`](https://developer.close.com/api/resources/unsubscribed-emails/delete) | **Raw JSON** |
| unsubscribed_emails | GET | [`/unsubscribe/email/`](https://developer.close.com/api/resources/unsubscribed-emails/list) | **Raw JSON** |
| unsubscribed_emails | POST | [`/unsubscribe/email/`](https://developer.close.com/api/resources/unsubscribed-emails/create) | **Raw JSON** |
| users | GET | [`/me/`](https://developer.close.com/api/resources/users/get-me) | `Users.GetMeAsync` |
| users | GET | [`/user/`](https://developer.close.com/api/resources/users/list) | **Raw JSON** |
| users | GET | [`/user/availability/`](https://developer.close.com/api/resources/users/list-availabilities) | **Raw JSON** |
| users | GET | [`/user/{id}/`](https://developer.close.com/api/resources/users/get) | **Raw JSON** |
| webhooks | DELETE | [`/webhook/{id}/`](https://developer.close.com/api/resources/webhooks/delete) | `Webhooks.DeleteAsync` |
| webhooks | GET | [`/webhook/`](https://developer.close.com/api/resources/webhooks/list) | `Webhooks.ListPageAsync` |
| webhooks | GET | [`/webhook/{id}/`](https://developer.close.com/api/resources/webhooks/get) | `Webhooks.GetAsync` |
| webhooks | POST | [`/webhook/`](https://developer.close.com/api/resources/webhooks/create) | `Webhooks.CreateAsync` |
| webhooks | PUT | [`/webhook/{id}/`](https://developer.close.com/api/resources/webhooks/update) | `Webhooks.UpdateAsync` |
