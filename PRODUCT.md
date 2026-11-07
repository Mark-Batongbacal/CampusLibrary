# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Librarians managing the library's catalog, member records, and borrowing/return activity. Android users browse and borrow books through a separate app.

## Product Purpose

CampusLibrary is a library management system. Success means librarians can manage books and members and administer borrowing records, while Android users can browse and borrow books.

## Operating Context

The librarian interface is a server-rendered ASP.NET Core MVC web application backed directly by the CampusLibrary EF Core data layer. The Android application uses a separate Web API.

## Capabilities and Constraints

The initial domain includes Books, Members, and Loans. A book may have at most one open loan. Authentication is not yet defined. Development uses .NET 9, EF Core, and SQL Server 2022 in Docker. No product-specific branding or visual assets have been established.

## Evidence on Hand

The repository contains the project requirements in `CLAUDE.md`, existing domain entities and API behavior under `src/`, and no established MVC UI or branded visual assets. Do not invent branding or present development seed data as real library records.

## Product Principles

- Keep librarian workflows direct and understandable.
- Preserve borrowing and return history.
- Keep API, MVC, and Android responsibilities distinct.
- Use server-side validation and database constraints for correctness.
