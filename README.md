# 🔗 URL Shortener System (React + .NET + PostgreSQL + Redis)

A full-stack production-ready URL Shortener application built using:

* ⚛️ React (Frontend)
* 🟣 ASP.NET Core Web API (.NET)
* 🐘 PostgreSQL (Neon DB)
* ⚡ Redis (Caching + Rate Limiting)
* 🐳 Docker
* ☁️ Render Deployment

---

# 🚀 Live Demo

Frontend: https://urlshortner-react-net.vercel.app/login

Backend API: https://urlshortner-react-net.onrender.com/

---

# 📌 Features

## ✅ Authentication & Authorization

* User Registration
* User Login
* JWT Authentication
* Protected Routes
* Logout Functionality

---

## ✅ URL Shortening

* Generate short URLs
* Base62 encoding
* Custom Alias Support (Premium Only)
* Expiry Date Support
* Redirect to original URL

Example:

https://domain.com/abc123

---

## ✅ Analytics

* Click Count Tracking
* Created Date
* Expiry Tracking
* URL Analytics API

---

## ✅ QR Code System

* Generate QR Code
* Download QR Code
* Share shortened URLs easily

---

## ✅ Dashboard

* List all URLs
* Search URLs
* Filter URLs
* Total Click Count
* Active URLs Count
* Expired URLs Count
* Top Performing URL

---

## ✅ Pagination

Implemented backend pagination using:

* Skip()
* Take()

Supports:

* Current Page
* Total Pages
* Page Size
* Total Records

---

## ✅ Edit URL Feature

Users can:

* Update Original URL
* Update Expiry Date
* Update Custom Alias (Premium Only)

---

## ✅ Delete URL

Users can permanently delete URLs.

---

## ✅ User Profile Page

Shows:

* Email
* Subscription Plan
* Member Since
* Total URLs
* Total Clicks

Includes:

* Premium Badge ⭐
* Upgrade Button
* Dashboard Navigation

---

## ✅ Premium System

Premium users can:

* Create Custom Aliases
* Edit Aliases

Example:

https://domain.com/harshit

instead of:

https://domain.com/a1b2c3

---

## ✅ Redis Integration

Redis is used for:

### 1. URL Caching

Frequently accessed URLs are stored in Redis to reduce database queries.

### 2. Rate Limiting

Limits excessive requests.

Example:

* Maximum 5 shorten requests per minute

---

## ✅ Rate Limiting

Prevents API abuse using Redis.

Implemented using:

* User IP Address
* Redis Increment Counter
* Expiry Window

Response:

HTTP 429 Too Many Requests

---

## ✅ Dockerized Application

Project includes:

* Frontend Dockerfile
* Backend Dockerfile
* docker-compose.yml

Services:

* Frontend
* Backend
* PostgreSQL
* Redis

---

# 🛠️ Tech Stack

## Frontend

* React
* React Router DOM
* Axios
* JWT Decode
* QRCode.react

---

## Backend

* ASP.NET Core Web API
* Entity Framework Core
* JWT Authentication
* PostgreSQL
* Redis

---

## Database

* PostgreSQL (Neon)

---

## Cache

* Redis
* Upstash Redis (Production)

---

## Deployment

* Render
* Docker

---

# 📂 Project Structure

## Frontend

url-shortner-client/

src/
├── api/
├── components/
├── pages/
├── styles/
├── utils/

---

## Backend

UrlShortner/

Controllers/
├── AuthController.cs
├── UrlController.cs

Data/
├── AppDbContext.cs

DTOs/
Models/
Helpers/

---

# 🔐 Authentication Flow

## Registration

1. User enters email/password
2. Password hashed using BCrypt
3. User stored in database

---

## Login

1. Validate credentials
2. Generate JWT token
3. Store token in localStorage

---

## Protected APIs

JWT token sent using:

Authorization: Bearer TOKEN

---

# 🔄 URL Shortening Flow

1. User submits URL
2. Backend validates request
3. URL stored in PostgreSQL
4. Base62 short code generated
5. Short URL returned

---

# ⚡ Redis Cache Flow

## Redirect Request

1. User opens short URL
2. Backend checks Redis

### Cache Hit

* Redirect immediately

### Cache Miss

* Fetch from PostgreSQL
* Store in Redis
* Redirect user

---

# 📈 Analytics Flow

Each redirect:

* Increases ClickCount
* Saves analytics data

---

# 📦 API Endpoints

# Auth APIs

## Register

POST /api/auth/register

---

## Login

POST /api/auth/login

---

## Upgrade Premium

POST /api/auth/upgrade

---

## Get Profile

GET /api/auth/profile

---

# URL APIs

## Shorten URL

POST /api/url/shorten

---

## Redirect URL

GET /{shortCode}

---

## Get Analytics

GET /api/url/analytics/{shortCode}

---

## Get My URLs

GET /api/url/myurls

Supports:

?page=1&pageSize=10

---

## Update URL

PUT /api/url/{id}

---

## Delete URL

DELETE /api/url/{id}

---

# 🐳 Docker Setup

## Run Project

docker-compose up --build

---

## Stop Project

docker-compose down

---

# ☁️ Deployment Steps

## Backend Deployment

Platform: Render

### Environment Variables

ConnectionStrings__DefaultConnection

Jwt__Key

Jwt__Issuer

Jwt__Audience

Redis__ConnectionString

---

## Frontend Deployment

Platform: Render / Vercel

Update API base URL in Axios.

---

# 🧠 Important Concepts Learned

## Backend

* REST APIs
* JWT Authentication
* Entity Framework Core
* Middleware
* DTOs
* Dependency Injection
* Redis
* Rate Limiting
* Pagination

---

## Frontend

* React Hooks
* State Management
* API Integration
* Protected Routes
* Conditional Rendering
* QR Code Integration

---

## DevOps

* Docker
* Docker Compose
* Render Deployment
* Environment Variables

---

# 📚 Future Improvements

* Role-Based Authentication
* Admin Dashboard
* URL Expiry Notifications
* Password Reset
* Email Verification
* Custom Domain Support
* Advanced Analytics Charts
* Kubernetes Deployment
* CI/CD Pipeline
* Microservices Architecture

---

# 🎯 Interview Questions Covered

## Backend

* What is JWT?
* Why use DTOs?
* What is Dependency Injection?
* What is Redis?
* How does caching work?
* What is Rate Limiting?
* What is Pagination?
* What is Docker?

---

## Database

* Why PostgreSQL?
* What is indexing?
* Why use Redis with SQL?

---

## Frontend

* What are React Hooks?
* What is useEffect?
* What is Axios?
* How does Protected Routing work?

---

# 👨‍💻 Author

Harshit Goel

GitHub:
https://github.com/GoHarshit

---

# ⭐ Final Notes

This project demonstrates:

* Full Stack Development
* Authentication
* System Design Basics
* Caching
* Scalability Concepts
* Deployment
* Dockerization
* Production-Level Architecture

This is a strong resume-level MERN/.NET full-stack project suitable for:

* Product Companies
* SDE Interviews
* Internship Applications
* Portfolio Showcase
