# Copilot Instructions

## Project Overview

This is a C# WinForms desktop application that stores data locally using SQLite and Entity Framework Core.

The codebase should remain simple, lightweight, and maintainable. Prefer straightforward implementations and avoid unnecessary abstractions.

---

## Technology Stack

Language: C#  
Framework: .NET WinForms  
Database: SQLite  
ORM: Entity Framework Core

Use Entity Framework Core with the SQLite provider for all database interactions.

Avoid:
- Raw SQL unless strictly necessary
- Other ORMs
- Web frameworks
- Heavy external dependencies

---

## Project Structure

The project follows a simple layered structure:

/Models  
/Data  
/Services  
/Forms  

---

### Models

Models represent data entities used by the application.

Rules:
- Plain C# classes (POCO)
- Properties only
- No UI logic
- No database queries

Example:

public class Entity
{
    public int Id { get; set; }
}

---

### Data

Responsible for database configuration.

Contains:
- AppDbContext class
- Entity Framework configuration

Example:

public class AppDbContext : DbContext
{
    public DbSet<Entity> Entities { get; set; }
}

Rules:
- Use DbSet<T> for entities
- Use LINQ queries
- Use SaveChanges() to persist changes

---

### Services

Contains application logic that should not exist in the UI layer.

Responsibilities may include:
- Data processing
- Calculations
- Coordinating database operations

Services may use AppDbContext.

---

### Forms

WinForms UI layer.

Responsibilities:
- Handle user interaction
- Display and edit data
- Call services or DbContext

Forms should not contain complex business logic.

Use standard WinForms controls such as:
- DataGridView
- TextBox
- ComboBox
- NumericUpDown
- Button

---

## Entity Framework Guidelines

- Use DbContext as the main access point to the database
- Use LINQ for querying
- Keep queries simple and readable
- Avoid complex EF configuration unless necessary

Prefer dependency injection or controlled creation of DbContext.

---

## Coding Guidelines

- Prefer clear and readable code
- Keep classes small and focused
- Follow standard C# naming conventions

Naming conventions:

PascalCase → classes, methods, properties  
camelCase → local variables

---

## General Principles

- Separate UI, data access, and application logic
- Keep responsibilities clear between layers
- Favor maintainability over clever solutions