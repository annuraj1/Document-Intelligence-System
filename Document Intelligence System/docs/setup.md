# AI Document Intelligence Notebook - Setup Guide

## 1) Prerequisites

- .NET 8 SDK
- MongoDB Atlas account and cluster
- Adobe PDF Extract API credentials
- Google Gemini API key

## 2) Configure Environment

Update values in `.env` at project root:

- `MONGODB_CONNECTION_STRING`
- `MONGODB_DATABASE_NAME`
- `ADOBE_CLIENT_ID`
- `ADOBE_CLIENT_SECRET`
- `ADOBE_EXTRACT_ENDPOINT`
- `GEMINI_API_KEY`
- `GEMINI_MODEL`
- `FRONTEND_BASE_URL`

## 3) Backend Run

```powershell
cd backend
dotnet restore
dotnet run
```

Backend runs with Swagger enabled.

## 4) Frontend Run

```powershell
cd frontend
dotnet restore
dotnet run
```

Open the printed frontend URL in browser.

## 5) API Flow

1. Upload PDF with `POST /upload`
2. Run extraction with `POST /extract`
3. Run content analysis with `POST /analyze`
4. Run AI summary with `POST /summarize`
5. View notebook details with `GET /document/{id}`

## 6) MongoDB Collections

- `Users`
- `Documents`
- `ExtractedContent`
- `Summaries`
