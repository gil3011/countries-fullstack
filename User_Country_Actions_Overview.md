# 🌍 User Actions on Countries Overview

This document provides a quick, easy-to-read overview of all the actions a user can perform regarding **Countries** in the system.

## 🧭 Country Exploration & Data

| Action | Description | Endpoint |
| :--- | :--- | :--- |
| **View All Countries** | Retrieve the full list of all available countries in the system. | `GET /api/Country` |
| **View Specific Country** | Search and view detailed info of a specific country by its CCA3 code (e.g., ISR). | `GET /api/Country/GetByCca3?cca3={code}` |

---

## 📌 Personal Tracking Lists

| Action | Description | Endpoint |
| :--- | :--- | :--- |
| **Add to Visited** | Mark a country as "Visited" (I have been there). | `POST /api/User/{userId}/visited/{countryId}` |
| **Remove from Visited** | Remove a country from the "Visited" list. | `DELETE /api/User/{userId}/visited/{countryId}` |
| **View Visited List** | Get the list of all countries the user has visited. | `GET /api/User/{userId}/visited` |
| **Add to Wishlist** | Mark a country as "Wishlist" (I want to go there). | `POST /api/User/{userId}/wishlist/{countryId}` |
| **Remove from Wishlist** | Remove a country from the "Wishlist" list. | `DELETE /api/User/{userId}/wishlist/{countryId}` |
| **View Wishlist** | Get the list of all countries the user wants to visit. | `GET /api/User/{userId}/wishlist` |
| **Move to Visited** | Move a country from the Wishlist directly to the Visited list. | `POST /api/User/{userId}/moveToVisited/{countryId}` |

---

## ✍️ Social & Community (Shares/Posts)

| Action | Description | Action Details |
| :--- | :--- | :--- |
| **Write a Post (Share)** | Write a review, tip, or post about a specific country. | Saves a `Share` object linked to `CountryId` |
| **Read Country Posts** | View what other users have shared or written about a specific country. | Calls `DBServiceShare.GetCountryShares(countryName)` |
| **Edit / Delete Post** | A user can modify or delete their own posts about a country. | Updates or deletes the specific `Share` |

---

> [!NOTE] 
> Admin users have additional capabilities such as **Adding, Updating, or Deleting** countries entirely from the main database (`POST`, `PUT`, `DELETE` via `CountryController`).
