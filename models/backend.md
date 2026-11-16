# Endpoints

All protected requests include:

`Authorization: Bearer <token>`

JWT should contain at least the user_id and role(`ADMIN` or `USER`).

## Public endpoints

| Method | Path | Purpose | Notes |
|---|---|---|---|
| POST | `/api/auth/login` | Log in with username and password | Returns JWT |

## User endpoints

| Method | Path | Purpose | Notes |
|---|---|---|---|
| GET | `/api/materials` | List materials visible to the current user | Returns only materials the user is allowed to access. |
| GET | `/api/materials/{materialId}` | Fetch one material by id | Checks `user_material_access` before returning. |
| POST | `/api/users/me/password` | Change the current user's password | Body: `{ "currentPassword": "...", "newPassword": "..." }` |

## Admin endpoints

| Method | Path | Purpose | Notes |
|---|---|---|---|
| GET | `/api/admin/users` | Get a list of all users | - |
| POST | `/api/admin/users` | Create a new user | Body: `{ "username": "student1", "temporaryPassword": "..." }` |
| DELETE | `/api/admin/users/{userId}` | Delete a user | Removes the user and related access assignments. |
| PUT | `/api/admin/users/{userId}/password` | Change a user's password | Body: `{ "newPassword": "..." }` |
| POST | `/api/admin/materials` | Create a material | Create text or image material. |
| PUT | `/api/admin/materials/{materialId}` | Update a material | Update metadata/content. |
| DELETE | `/api/admin/materials/{materialId}` | Delete a material | Removes related material and grant data. |
| GET | `/api/admin/materials/{materialId}/access` | Read material access grants | Returns the list of current allowed users. |
| PUT | `/api/admin/materials/{materialId}/access` | Set material access | - |
