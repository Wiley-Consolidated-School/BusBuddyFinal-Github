# Codecov Setup Instructions

## Setting up Codecov Token for GitHub Actions

To resolve the Codecov rate limit issues, you need to set up a Codecov upload token:

### Step 1: Get Your Codecov Token
1. Go to [Codecov.io](https://codecov.io/) and sign in with your GitHub account
2. Navigate to your repository: `Bigessfour/BusBuddyHelper`
3. Go to Settings → Repository Upload Token
4. Copy the upload token

### Step 2: Add Token to GitHub Secrets
1. Go to your GitHub repository: `https://github.com/Bigessfour/BusBuddyHelper`
2. Click on Settings → Secrets and variables → Actions
3. Click "New repository secret"
4. Name: `CODECOV_TOKEN`
5. Value: Paste the token from Codecov
6. Click "Add secret"

### Step 3: Verify Setup
After adding the token, your GitHub Actions workflow will automatically use it for uploads, avoiding rate limits.

## Codecov Configuration

The `.codecov.yml` file has been created with the following settings:
- Target coverage: 80%
- Precision: 2 decimal places
- Supports both project and patch coverage
- Configured for C# projects with Cobertura XML format

## Benefits
- **Rate Limit Resolution**: Using a token prevents hitting public rate limits
- **Better Reporting**: More detailed coverage reports and trends
- **PR Comments**: Automatic coverage reports on pull requests
- **Historical Tracking**: Coverage trends over time
