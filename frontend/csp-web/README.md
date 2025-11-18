# Lector Library - Frontend

This is the frontend for the Lector Library project, a comprehensive library management system. It is a single-page application built with React and Vite.

## Features

-   **Book Catalog:** Browse and search for books in the library.
-   **User Authentication:** Login and registration for library members.
-   **Member Dashboard:** View loan history, fines, and manage reservations.
-   **Admin Dashboard:** Manage books, members, and loans.

## Getting Started

### Prerequisites

-   Node.js and npm

### Installation

1.  **Navigate to the frontend directory:**
    ```bash
    cd frontend/csp-web
    ```
2.  **Install dependencies:**
    ```bash
    npm install
    ```
3.  **Run the development server:**
    ```bash
    npm run dev
    ```
    The application will be available at `http://localhost:5173`.

## Project Structure

The `src` folder contains the main source code for the application, organized as follows:

-   `components`: Contains reusable UI components.
-   `pages`: Contains the main pages of the application.
-   `lib`: Contains the API client for communicating with the backend.
-   `styles`: Contains global styles and themes.

## API Integration

The frontend communicates with the backend API to fetch and update data. The API client is located in `src/lib/api.js`.

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue.
