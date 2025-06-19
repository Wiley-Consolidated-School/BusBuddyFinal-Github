# BusBuddy Setup Guide

## 🚀 **Quick Start**

### **Prerequisites**
- Windows 10/11 (64-bit)
- .NET 8.0 Runtime or SDK
- SQL Server or SQL Server Express
- Visual Studio 2022 or VS Code (for development)

### **Installation Steps**

#### 1. **Clone Repository**
```bash
git clone https://github.com/your-org/BusBuddy.git
cd BusBuddy
```

#### 2. **Restore Dependencies**
```bash
dotnet restore BusBuddy.sln
```

#### 3. **Database Setup**
```bash
# Update connection string in App.config
# Run database initialization
dotnet run --project BusBuddy -- --init-db
```

#### 4. **Build and Run**
```bash
dotnet build BusBuddy.sln
dotnet run --project BusBuddy
```

## 🔧 **Configuration**

### **Database Connection**
Update `App.config` with your SQL Server connection string:
```xml
<connectionStrings>
  <add name="BusBuddyContext"
       connectionString="Server=.;Database=BusBuddy;Integrated Security=true;TrustServerCertificate=true;"
       providerName="Microsoft.EntityFrameworkCore.SqlServer" />
</connectionStrings>
```

### **Syncfusion Licensing**
1. Obtain a Syncfusion Community License (free) from https://www.syncfusion.com/products/communitylicense
2. Update `syncfusion-license.json`:
```json
{
  "SyncfusionLicense": {
    "LicenseKey": "YOUR_LICENSE_KEY_HERE",
    "LicenseType": "Community",
    "Status": "Active"
  }
}
```

## 🧪 **Development Environment**

### **Required Extensions (VS Code)**
- C# Dev Kit
- .NET Extension Pack
- PowerShell Extension
- GitLens

### **Build Tasks**
Available VS Code tasks:
- `build BusBuddy` - Build the solution
- `test BusBuddy` - Run all tests
- `Generate Code Coverage` - Generate coverage reports

### **Database Development**
```bash
# Initialize test database
dotnet run --project BusBuddy -- --init-test-db

# Run migrations
dotnet ef database update --project BusBuddy.Data
```

## 🚨 **Troubleshooting**

### **Common Issues**

#### **"No Rendered Text" in Dashboard**
- Ensure Syncfusion license is properly configured
- Check DPI scaling settings
- Verify Segoe UI font availability

#### **Database Connection Errors**
- Verify SQL Server is running
- Check connection string format
- Ensure database exists and is accessible

#### **Build Errors**
- Run `dotnet clean` followed by `dotnet restore`
- Check .NET SDK version compatibility
- Verify all NuGet packages are restored

### **Performance Optimization**
- Enable SQL Server connection pooling
- Use async/await for database operations
- Implement lazy loading for large datasets

## 📞 **Support**

For issues and questions:
1. Check [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
2. Review existing GitHub issues
3. Create new issue with detailed description

---

**Setup Guide Version:** 2.0
**Last Updated:** June 18, 2025
