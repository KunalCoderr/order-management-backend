🧾 Overview

This is an Order Management System built using ASP.NET Framework 4.8.

🔧 Architecture

Follows the Repository Pattern using a Database-First Approach Flow:
Request → Controller → Service → Repository

⚙️ Technologies Used

1. Redis Caching
 - Used for caching product and order data.
 - Local Redis server is used (via Docker).

2. VB.NET Utility Library
 - Handles validation, logging, and string formatting.

3. Database
 - SQL script files are available in the DBScripts folder.
 - Use the latest version of the file named:
	OMS SQLQuery V1.0.sql (or newer versions if available) to restore the database.

🚀 Setup Instructions
Backend Setup (.NET)

1. Clone the backend solution.
2. Set up a local Redis server using Docker:
 - Make sure Docker Desktop is installed and running.
 - Open Command Prompt and run the following commands:

docker run -d --name "cache" -p 5000:6379 redis

docker ps

docker exec -it cache sh

redis-cli

select 0

ping

GET product_list


3. Restore the Database:
 - Use SQL Server Management Studio (SSMS) or any SQL tool.
 - Navigate to the DBScripts folder.
 - Use the latest OMS SQLQuery V1.0.sql script to restore the database.

Frontend Setup (Angular)

1. Install Node.js (https://nodejs.org/)
2. Install Angular CLI (if not already installed):
 - npm install -g @angular/cli
3. Clone the frontend application.
4. Navigate to the project root directory:
 - cd <your-frontend-folder>
5. Install project dependencies:
 - npm install
6. Run the Angular app:
 - ng serve

7. Open the app in your browser:
http://localhost:4200


⚡ Integration Details: Redis Caching & VB.NET Utility Library
🧠 Redis Caching Integration

After setting up the Redis local server, you need to configure the application to connect to it.

Steps:

1. Start Redis Server Locally
(Refer to the setup instructions above using Docker.)

2. Configure Connection in Application
 - Provide the Redis endpoint in your application's configuration (e.g., localhost:5000).
 - This allows the application to connect to Redis and perform cache operations.

Usage
 - Once connected, the application can store and retrieve data (such as product_list, order_details, etc.) using standard Redis commands.

🧰 VB.NET Utility Library Integration

The VB.NET utility library is used for handling common functionalities such as validation, logging, and string formatting.

Steps to Integrate:
1. Create a New VB.NET Class Library Project
 - Use Visual Studio to create a new VB.NET project of type Class Library.

2. Add Utility Modules
 - Create modules or classes for common reusable functions:

3. Reference the Utility Library in Main Project
 - Right-click on your main application project.
 - Choose Add Reference.
 - Select the VB.NET utility library project to link it.
