# Endpoints

All requests to non-public endpoints include: `Authorization: Bearer <token>`

JWT should contain at least the user_id and role(`ADMIN` or `USER`)

For `datetime` ISO 8601 string should be used

## Public endpoints(No JWT required in header)

| Method | Path | Purpose | Request | Response |
|---|---|---|---|---|
| POST | `/api/auth/login` | Log in with username and password | `{ username: text, password: text }` | `{ token: text }` |

## Auth endpoints

| Method | Path | Purpose | Request | Response |
|---|---|---|---|---|
| POST | `/api/auth/logout` | Log out the current user(No body, but JWT in header is passed) | - | - |
| GET | `/api/auth/me` | Get the current authenticated user | - | `{ userId: text, isAdmin: boolean, createdAt: datetime }` |

## User endpoints

| Method | Path | Purpose | Request | Response |
|---|---|---|---|---|
| GET | `/api/materials` | List materials visible to the current user | - | `[{ materialId: text, materialType: text, content: text, createdAt: datetime }]` |
| GET | `/api/materials/{materialId}` | Fetch one material by id | `materialId` in the path | `{ materialId: text, materialType: text, content: text, createdAt: datetime) }` |
| POST | `/api/users/me/password` | Change the current user's password | `{ newPassword: text }` | - |

## Admin endpoints

| Method | Path | Purpose | Request | Response |
|---|---|---|---|---|
| GET | `/api/admin/users` | Get a list of all users | - | `[{ userId: text, isAdmin: boolean, createdAt: datetime }]` |
| POST | `/api/admin/users` | Create a new user | `{ userId: text, temporaryPassword: text }` | `{ userId: text, isAdmin: boolean, createdAt: datetime }` |
| DELETE | `/api/admin/users/{userId}` | Delete a user | `userId` in the path | - |
| PUT | `/api/admin/users/{userId}/password` | Change a user's password | `{ newPassword: text }`; `userId` in the path | - |
| POST | `/api/admin/materials` | Create a material | `{ materialId: text, materialType: text, content: text }` | `{ materialId: text, materialType: text, content: text, createdAt: datetime }` |
| PUT | `/api/admin/materials/{materialId}` | Update a material | `{ materialType: text, content: text }`; `materialId` in the path | `{ materialId: text, materialType: text, content: text, createdAt: datetime }` |
| DELETE | `/api/admin/materials/{materialId}` | Delete a material | `materialId` in the path | - |
| GET | `/api/admin/materials/{materialId}/access` | Read material access grants | `materialId` in the path | `[text]` |
| PUT | `/api/admin/materials/{materialId}/access` | Set material access | `{ userIds: [text] }`; `materialId` in the path | - |
