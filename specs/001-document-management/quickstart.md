# Quickstart Guide: Document Upload and Management

## Running the Application Locally

1. **Verify Environment**:
   ```bash
   dotnet --version
   ```
2. **Restore and Build**:
   ```bash
   cd ContosoDashboard
   dotnet restore
   dotnet build
   ```
3. **Run Application**:
   ```bash
   dotnet run
   ```
4. **Access the Dashboard**:
   - Navigate to `https://localhost:5001` or `http://localhost:5000` in your web browser.
   - On the login screen, select **Ni Kang (Employee)** and click **Login**.
   - Select **Documents** from the left navigation sidebar.
   - Click **Upload Document**, fill in Title, Category, choose a file, and click **Upload**.
   - Your uploaded file will appear under **My Documents**.
   - Click **Download** to retrieve the file from secure storage.
