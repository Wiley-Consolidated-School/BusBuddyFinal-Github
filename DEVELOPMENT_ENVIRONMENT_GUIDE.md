# 🛠️ BusBuddy Development Environment Guide
## Complete Toolset Configuration & Usage

Your VS Code environment is now fully configured with professional development tools. Here's how to use them effectively:

---

## 🚀 **QUICK START GUIDE**

### Daily Development Workflow:
1. **Open BusBuddy workspace** in VS Code
2. **Pull latest changes**: `Ctrl+Shift+P` → "Git: Pull"
3. **Build project**: `Ctrl+Shift+P` → "Tasks: Run Task" → "build BusBuddy"
4. **Run tests**: `Ctrl+Shift+P` → "Tasks: Run Task" → "test BusBuddy"
5. **Start debugging**: `F5` or `Ctrl+F5`

---

## 🎯 **ESSENTIAL TOOLS & SHORTCUTS**

### **C# Development:**
- **IntelliSense**: `Ctrl+Space` (auto-completion)
- **Go to Definition**: `F12`
- **Find References**: `Shift+F12`
- **Rename Symbol**: `F2`
- **Format Document**: `Shift+Alt+F`
- **Quick Fix**: `Ctrl+.`

### **Debugging:**
- **Start Debugging**: `F5`
- **Start Without Debugging**: `Ctrl+F5`
- **Toggle Breakpoint**: `F9`
- **Step Over**: `F10`
- **Step Into**: `F11`
- **Continue**: `F5`

### **Testing:**
- **Run All Tests**: Use Test Explorer panel
- **Run Specific Test**: Click ▶️ next to test method
- **Generate Coverage**: `Ctrl+Shift+P` → "Tasks: Run Task" → "Generate Code Coverage"
- **View Coverage**: `Ctrl+Shift+C`

### **Git Integration:**
- **Source Control Panel**: `Ctrl+Shift+G`
- **Commit Changes**: `Ctrl+Enter` (in Source Control)
- **View File History**: Click file → GitLens → "Open File History"
- **Compare Changes**: Click file → "Compare with Previous"

---

## 📊 **CODE QUALITY TOOLS**

### **1. SonarLint (Already Configured)**
- **Real-time code analysis**
- **Highlights issues** as you type
- **Provides fixing suggestions**
- **Access**: Problems panel (`Ctrl+Shift+M`)

### **2. Coverage Gutters**
- **View test coverage** directly in editor
- **Toggle coverage**: `Ctrl+Shift+C`
- **Green lines**: Covered code
- **Red lines**: Uncovered code
- **Orange lines**: Partially covered

### **3. Code Spell Checker (Newly Installed)**
- **Checks spelling** in comments and strings
- **Quick fix**: `Ctrl+.` on misspelled words
- **Add to dictionary**: Right-click → "Add to dictionary"

---

## 🔧 **ADVANCED FEATURES**

### **1. Enhanced IntelliSense:**
```csharp
// Type hints for implicit variables
var customer = new Customer(); // Shows actual type
```

### **2. Code Actions:**
- **Generate constructors**: `Ctrl+.` in class
- **Implement interface**: `Ctrl+.` on interface name
- **Extract method**: Select code → `Ctrl+.`
- **Add using statements**: `Ctrl+.` on unknown type

### **3. Refactoring Tools:**
- **Rename Symbol**: `F2` (renames across entire solution)
- **Move to namespace**: `Ctrl+.` on class
- **Extract interface**: `Ctrl+.` on class

---

## 🎨 **UI DEVELOPMENT FEATURES**

### **Windows Forms Designer:**
- **Form files** open in designer by default
- **Toolbox**: `Ctrl+Alt+X`
- **Properties**: `F4`
- **Code-behind**: `F7`

### **Material Design Support:**
- **IntelliSense** for Material components
- **Color previews** in CSS/styling
- **Icon previews** in code

---

## 🗃️ **DATABASE TOOLS**

### **SQL Server Extension Features:**
- **Connection Manager**: `Ctrl+Shift+P` → "MS SQL: Connect"
- **Query Editor**: `.sql` files have syntax highlighting
- **Execute Query**: `Ctrl+Shift+E`
- **View Results**: Results panel opens automatically

### **Database Operations:**
```sql
-- Your connection is pre-configured for BusBuddy database
SELECT * FROM Vehicles WHERE Status = 'Active'
```

---

## 📈 **PERFORMANCE MONITORING**

### **Built-in Performance Tools:**
- **Diagnostic Tools**: Available during debugging
- **Memory usage**: Shows in debug session
- **CPU usage**: Shows in debug session

### **Code Metrics:**
- **Complexity analysis**: Problems panel
- **Performance suggestions**: SonarLint provides recommendations

---

## 🔍 **SEARCH & NAVIGATION**

### **Enhanced Search:**
- **Go to File**: `Ctrl+P`
- **Go to Symbol**: `Ctrl+Shift+O`
- **Search in Files**: `Ctrl+Shift+F`
- **Find References**: `Shift+F12`

### **Smart Navigation:**
- **Breadcrumbs**: Shows file structure
- **Outline View**: Shows file symbols
- **Explorer**: Enhanced file tree

---

## 🚦 **PROBLEM SOLVING**

### **Error Resolution:**
1. **Check Problems Panel**: `Ctrl+Shift+M`
2. **Use Quick Fix**: `Ctrl+.` on red squiggle
3. **Check SonarLint suggestions**
4. **Use IntelliSense hints**

### **Debug Issues:**
1. **Set breakpoints** at problem areas
2. **Use Watch window** for variable inspection
3. **Check Call Stack** for execution flow
4. **Use Immediate window** for testing

---

## 📋 **DAILY CHECKLIST**

### **Before Starting Work:**
- [ ] Pull latest changes (`Ctrl+Shift+P` → "Git: Pull")
- [ ] Build solution (`Ctrl+Shift+B`)
- [ ] Run tests to ensure baseline

### **While Coding:**
- [ ] Watch for SonarLint suggestions
- [ ] Use `Ctrl+.` for quick fixes
- [ ] Save frequently (`Ctrl+S`)
- [ ] Run relevant tests

### **Before Committing:**
- [ ] Build solution successfully
- [ ] Run all tests
- [ ] Check code coverage
- [ ] Review changed files
- [ ] Write meaningful commit message

---

## 🆘 **TROUBLESHOOTING**

### **Common Issues:**

**"IntelliSense not working"**
- Solution: `Ctrl+Shift+P` → "Developer: Reload Window"

**"Tests not discovered"**
- Solution: `Ctrl+Shift+P` → "Test: Reset and Reload All Test Data"

**"Coverage not showing"**
- Solution: Generate coverage first, then `Ctrl+Shift+C`

**"Git integration issues"**
- Solution: Check Git credentials in terminal

---

## 🎯 **NEXT STEPS**

1. **Familiarize yourself** with keyboard shortcuts
2. **Run your first debug session** using `F5`
3. **Generate code coverage** and review results
4. **Explore GitLens features** for code history
5. **Use SonarLint** to improve code quality

Your development environment is now professional-grade and ready for serious development work! 🚀
