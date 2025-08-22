# 💾 DatabasesBackupServiceDotNet  

[🇵🇱 Read in Polish](README_PL.md)

---

![.NET](https://img.shields.io/badge/.NET-9-blueviolet?logo=dotnet&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-purple?logo=blazor&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Ready-blue?logo=docker&logoColor=white)
![Database](https://img.shields.io/badge/Databases-MySQL%20|%20PostgreSQL%20|%20SQLite-lightgrey)
![License](https://img.shields.io/badge/License-MIT-green)

---

**DatabasesBackupServiceDotNet** is an open-source, containerized application built with **.NET 9 Blazor WebAssembly** using a modular monolith architecture.  
The application allows **centralized management of database backups across multiple servers** – currently supporting **MySQL** and **PostgreSQL**, with **MS SQL Server** planned.  

---

## 🔑 Key Features  

- 📅 Schedule automatic backups  
- 🔐 Encryption and compression of backup files  
- 📊 Backup history and statistics  
- 📧 Email notifications about backup status  
- 👥 **Groups and roles system** – the administrator can create groups with specific permissions, assign them to databases, and allow users to access (e.g., view the latest backup of a given database)  
- 🗄️ Configuration and management data are stored in **SQLite** (lightweight embedded database)  

---

## 🌟 Why this app?  

🔹 Most free backup tools only support 1–2 databases and require paid upgrades for more advanced features.  
🔹 This application was created as a **fully free and flexible solution** for managing multiple databases in one place.  
🔹 It also provides **security (encryption, authorization)** and **easy user management**.  

---

## 🏗️ System Architecture  

- **UI** – Blazor WebAssembly  
- **API/Server** – business logic, backup engine, database communication  
- **Application Database** – SQLite (users, groups, configurations, backup history)  
- **Authorization** – users, roles, groups linked to specific databases  
- **Backup Engine** – generation, encryption, compression, archiving of backups  
- **Notifications** – email system (statuses, reminders, errors)  
- **Testing** – option to restore and verify backups in a temporary test environment  

---

## 🚀 Setup & Configuration  

Go to the **WIKI** section and follow the setup instructions.  

---

## 🗺️ Roadmap  

### ✅ Completed  
- Support for **MySQL**  
- Support for **PostgreSQL**  
- **UI Panel** in Blazor  
- User, role & group management system  
- Backup scheduling  
- Email notifications  
- Configuration storage in **SQLite**  

### 🛠️ In Progress / Planned  
- Support for **MS SQL Server**  
- Advanced backup testing (temporary containers + SQL check queries)  
- Statistics and reporting in UI  
- Cloud storage integration (AWS S3, Azure Blob, GCP Storage)  

---

## 📜 License  
This project is released under the MIT license.<br>
You are free to use it commercially and privately, extend it, and adapt it to your needs.  

---

## 🤝 Contributing  
Want to contribute?<br>
Open an issue 🐛 in [Issues](../../issues)<br>
Propose a feature 💡<br>
Submit a pull request 🚀
