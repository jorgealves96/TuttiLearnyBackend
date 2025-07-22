#!/bin/bash

# This command applies any pending migrations
dotnet ef database update

# This command starts your actual application
dotnet LearningAppNetCoreApi.dll