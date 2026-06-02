# JwtDemo — .NET 8 Web API + JWT

Project học JWT authentication từ đầu.

## Chạy project

```bash
dotnet run
# Swagger UI: http://localhost:5244/swagger
```

## Stack

- .NET 8 Web API
- `Microsoft.AspNetCore.Authentication.JwtBearer` 8.x
- `BCrypt.Net-Next` 4.x — hash password
- `Swashbuckle.AspNetCore` 6.x — Swagger UI

## Cấu trúc

```
Models/
  User.cs          — entity User (Id, Username, PasswordHash, Role)
  AuthModels.cs    — DTOs: LoginRequest, RegisterRequest, RefreshRequest, TokenResponse

Services/
  JwtService.cs    — tạo/validate access token và refresh token
  UserService.cs   — in-memory user store + refresh token store

Controllers/
  AuthController.cs      — POST /auth/register, /auth/login, /auth/refresh
  ResourceController.cs  — GET /resource/public, /user, /admin, /me
```

## Endpoints

| Method | Path | Auth | Mô tả |
|---|---|---|---|
| POST | `/auth/register` | — | Tạo tài khoản mới |
| POST | `/auth/login` | — | Đăng nhập, nhận access + refresh token |
| POST | `/auth/refresh` | — | Đổi refresh token lấy cặp token mới |
| GET | `/resource/public` | — | Ai cũng truy cập được |
| GET | `/resource/user` | Bearer | Cần JWT hợp lệ |
| GET | `/resource/admin` | Bearer + Admin | Chỉ role Admin |
| GET | `/resource/me` | Bearer | Xem tất cả claims trong token |

## Seed users

| Username | Password | Role |
|---|---|---|
| admin | Admin@123 | Admin |
| alice | Alice@123 | User |

## Cấu hình JWT (appsettings.json)

| Key | Giá trị |
|---|---|
| Secret | `super-secret-key-that-must-be-at-least-32-chars!!` |
| Issuer | `JwtDemo` |
| Audience | `JwtDemoUsers` |
| Access token | 15 phút |
| Refresh token | 7 ngày |

> **Production:** Đổi `Secret` thành biến môi trường, thay `UserService` bằng DB thật (EF Core).

## Luồng hoạt động

```
1. POST /auth/login  →  { accessToken, refreshToken }
2. GET  /resource/*  →  Header: Authorization: Bearer <accessToken>
3. Access token hết hạn (15 phút)
4. POST /auth/refresh  →  { accessToken mới, refreshToken mới }  (token rotation)
```

## Những khái niệm đã implement

- **Access token** — JWT signed HMAC-SHA256, chứa claims (userId, username, role), hết hạn sau 15 phút
- **Refresh token** — chuỗi random 64 bytes, lưu server-side, không encode dữ liệu vào token
- **Token rotation** — mỗi lần refresh thì token cũ bị revoke, tránh token bị dùng lại
- **`[Authorize]`** — middleware tự validate JWT trên mọi request có attribute này
- **`[Authorize(Roles = "Admin")]`** — kiểm tra claim `role` trong token
- **`ClockSkew = TimeSpan.Zero`** — token hết hạn đúng giờ, không cho phép sai lệch đồng hồ
