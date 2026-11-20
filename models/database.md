## Database

```mermaid
erDiagram
    USER {
        varchar user_id PK
        varchar password_hash
        boolean is_admin
        timestamp created_at
    }

    MATERIAL {
        varchar material_id PK
        varchar type
        timestamp created_at
    }

    TEXT_MATERIAL {
        varchar text_id PK, FK
        text content
    }

    IMAGE_MATERIAL {
        varchar image_id PK, FK
        varchar s3_object_key
    }

    USER_MATERIAL_ACCESS {
        varchar user_id PK, FK
        varchar material_id PK, FK
    }

    USER ||--o{ USER_MATERIAL_ACCESS : has
    MATERIAL ||--o{ USER_MATERIAL_ACCESS : granted_to
    MATERIAL ||--o| TEXT_MATERIAL : contains
    MATERIAL ||--o| IMAGE_MATERIAL : contains
```

- `material` stores common metadata for a lesson item.
- `text_material` and `image_material` are one-to-one extensions of the same material row.
- `image_material.s3_object_key` is the private S3 object key used to generate presigned URLs after authorization is checked.
- `user_material_access` is the many-to-many grant table: which user can see which material.
