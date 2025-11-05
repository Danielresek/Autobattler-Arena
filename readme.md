# Autobattler

Autobattler is an early-stage game project built with **C#**, **.NET**, and **Unity**.  
The goal is to create a real-time multiplayer arena where players can join lobbies, compete in automated battles, and watch AI-controlled characters fight it out.

> **Status:** Work in progress - core systems are still under development.

---

## Current Project Structure

### **1. Server.Web**

- Built using **ASP.NET Core (.NET 8)**.
- Handles the backend logic and real-time communication via **SignalR**.
- Currently includes:
  - A local web server
  - A basic HTML test page (`join.html`)

### **2. Shared**

- Contains shared C# models used by both backend and (future) Unity client.
- Example models:
  - `LobbyPlayer` (represents a player in a lobby)
  - `LobbyState` (tracks status and players in a lobby)
  - `Personality` enum (used to categorize AI behavior)

### **3. Arena.Unity**

- Unity project created for future development of the visual part of the game.
- Currently only contains the basic Unity LTS setup.
- Will later be connected to the backend to display actual battles and arena logic.

---

## Running the Project

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/Autobattler.git
   cd Autobattler
   ```
