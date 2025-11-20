# CMCS Lecturer Claim System — README




## Overview

This repository is a prototype **CMCS Lecturer Claim System** (ASP.NET MVC) that allows lecturers to submit claims and attach supporting documents. It is intentionally lightweight and designed for demonstration / small-team use — the app stores claims in memory and stores uploaded files encrypted on disk.

Key features added in this update:

* Encrypted per-claim file uploads (.pdf, .docx, .xlsx) stored under `EncryptedFiles/`.
* Per-claim document listing and download.
* Inline PDF preview (modal + iframe) from the Claim Details page.
* AJAX file upload with a Bootstrap progress bar (client-side progress UI).
* Actions column on the Track Claims page containing **Upload** and **View** buttons.
* Secure-ish prototype encryption (AES + PBKDF2 with SHA-256) for stored files.

> Note: This is a prototype. The encryption key/passphrase is stored in `DocumentsController` for convenience; please follow the Security Recommendations below before using in production.

---

## Project layout (important files)

```
/CMCS.sln
/Controllers/
  - ClaimsController.cs        // Claim lifecycle (submit, list, details, etc.)
  - ApprovalsController.cs     // Approvals (coordinator & manager) — uses ClaimsController in-memory store
  - DocumentsController.cs     // Upload, encrypt, store, and decrypt/download files
/Models/
  - ClaimViewModel.cs         // Claim + Lines + Documents metadata
  - ClaimLineViewModel.cs
  - DocumentViewModel.cs
/Views/
  - Views/Claims/Track.cshtml    // Track page (Actions column added)
  - Views/Claims/Details.cshtml  // Claim details (shows documents + preview)
  - Views/Documents/Upload.cshtml// AJAX upload page with progress
/EncryptedFiles/                  // runtime folder where encrypted files are saved (created automatically)

wwwroot/                           // static assets
```

---

## Requirements

* [.NET SDK (6.0+ or 7.0+)](https://dotnet.microsoft.com) — open the solution in Visual Studio 2022/2023 or run with `dotnet run`.
* A modern browser for PDF preview (Edge, Chrome, Firefox, Safari).
* The app writes encrypted files to the `EncryptedFiles/` folder at the project root; ensure the app has write permission there.

---

## Running the app (quick start)

1. Unzip the project and open `CMCS.sln` in Visual Studio (or run from the command line).
2. Build the solution.
3. Run the app (F5 or `dotnet run` in the project folder).
4. The home route redirects to the Claims Submit page. Use the **Track Claims** view to see claimed items and access the new **Actions** column.

---

## Uploading and previewing documents (user flow)

1. From **Track Claims**, click **Upload** next to the claim you want to attach a document to.
2. On the upload page, choose a file (allowed types: `.pdf`, `.docx`, `.xlsx`; max 5 MB) and click **Upload**.
3. A progress bar will show upload progress. After completion, the page redirects to the claim Details.
4. In **Details**, the uploaded documents appear under *Supporting Documents*:

   * **Preview** (shown only for `.pdf`) will open a modal and display the decrypted PDF inline.
   * **Download** will download the decrypted file.

---

## Implementation notes

* **In-memory claims store**: Claims are still stored in memory (`ClaimsController` private `_claims` dictionary). This is a prototype choice and means claims are not persisted when the app restarts.
* **Encrypted file storage**: Files are encrypted using AES and saved as `{claimId}_{guid}.{ext}.enc` in `EncryptedFiles/`. The `DocumentsController` writes the IV followed by the ciphertext.
* **Key derivation**: The implementation uses `Rfc2898DeriveBytes` with SHA-256 and 100,000 iterations to derive the AES key from a passphrase.
* **Download / preview behavior**: PDFs are served in a manner suitable for inline browser display (so the `<iframe>` preview works). Other document types download as files.

---


