#!/bin/bash
dotnet ef dbcontext scaffold \
  "Server=localhost;Database=hospital;User Id=testuser;Password=testpass;" \
  Npgsql.EntityFrameworkCore.PostgreSQL \
  --output-dir Models \
  --context-dir . \
  --context HospitalContext  \
  --no-onconfiguring \
  --data-annotations \
  --force