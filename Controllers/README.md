# Ad Network Platform API & WordPress Dashboard

This project is a complete, full-stack ad network platform built with a .NET C# backend API and a WordPress plugin frontend. It provides a robust system for advertisers (Clients) to manage their campaigns and for website owners (Hosts) to display ads and earn revenue.

---

## Features

- **Role-Based User System**: Secure registration and login for three distinct user roles:
  - **Clients (Advertisers)**: Can create campaigns and upload rich media ads (Image + Text or Video + Text).
  - **Hosts (Publishers)**: Can browse all available ads and get simple iframe embed codes.
  - **Admins**: Have full access to both Client and Host dashboards, with the additional ability to delete any ad on the network.
- **Rich Media Ad Support**: The platform is designed to serve both image and video ads, complete with custom headlines and body text.
- **Secure API**: The backend is built with .NET, using JWT for secure, stateless authentication.
- **SQLite Database**: Uses a lightweight, file-based SQLite database for easy setup and portability (no complex database server required).
- **WordPress Integration**: The entire user-facing dashboard is a self-contained WordPress plugin, providing a familiar environment for users.

---

## Technology Stack

### Backend
- **C# with .NET**
- **ASP.NET Core Web API**
- **Entity Framework Core** for database management
- **SQLite** for the database
- **BCrypt.Net** for password hashing
- **JWT (JSON Web Tokens)** for authentication

### Frontend
- **WordPress** (as a plugin)
- **HTML5, CSS3, JavaScript**
- **Fetch API** for communicating with the backend

---

## Setup and Installation

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download)
- A local WordPress installation

### 1. Backend API Setup

1.  **Clone the repository:**
    ```bash
    git clone <your-repository-url>
    ```
2.  **Navigate to the project directory:**
    ```bash
    cd AdCampaignTracker
    ```
3.  **Restore dependencies:**
    ```bash
    dotnet restore
    ```
4.  **Run the application:**
    ```bash
    dotnet run
    ```
    The API will start and automatically create the `AdCampaignTracker.db` database file. It will be accessible at `http://localhost:5244`.

### 2. Frontend WordPress Plugin Setup

1.  **Copy the Plugin File**: Take the PHP file for the WordPress plugin and place it in your WordPress installation's `wp-content/plugins/` directory.
2.  **Activate the Plugin**: Log in to your WordPress admin dashboard, go to "Plugins", and activate the "Ad Network Dashboard" plugin.
3.  **Access the Dashboard**: A new "Ad Network" menu item will appear in your WordPress admin sidebar.

---

## Usage

1.  **Register Users**: Use the Swagger UI (`http://localhost:5244/swagger`) to access the `POST /api/Auth/register` endpoint and create your initial Admin, Client, and Host users.
2.  **Log In**: Access the "Ad Network" dashboard in WordPress and log in with the credentials you created.
3.  **Manage Content**:
    - As a **Client**, create campaigns and upload ads.
    - As a **Host**, browse the available ads and copy the embed code.
    - As an **Admin**, you can do both, plus delete ads from the Client dashboard view.
