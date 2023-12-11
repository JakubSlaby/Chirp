const fs = require('fs');
const path = require('path');
const gitConfigPath = path.join(__dirname, '../.git/config');
const packageJsonPath = path.join(__dirname, '../package.json');
const urlRegex = /[remote\s*"origin"]\s*url\s*=\s*(?<url>[^\s]+)/;


// Function to update the repository URL
function updateRepositoryUrl() {
    const configText = fs.readFileSync(gitConfigPath, 'utf8');
    if(configText == null || configText.length == 0)
    {
        console.error("Unable to find the git repository config file.");
        return 1;
    }

    const configUrlMatch = urlRegex.exec(configText);
    const configUrl = configUrlMatch.groups.url;
    if(configUrl == null || configUrl.length == 0)
    {
        console.error("Unable to find the repository URL");
        return 1;
    }
    const gitUrl = configUrl.replace("https://", "git://");
    const issuesUrl = configUrl.replace(".git", "/issues");
    
    // Read package.json
    var s = fs.readFileSync(packageJsonPath, 'utf8');

    // Preserve newlines, etc. - use valid JSON
    s = s.replace(/\\n/g, "\\n")
        .replace(/\\'/g, "\\'")
        .replace(/\\"/g, '\\"')
        .replace(/\\&/g, "\\&")
        .replace(/\\r/g, "\\r")
        .replace(/\\t/g, "\\t")
        .replace(/\\b/g, "\\b")
        .replace(/\\f/g, "\\f")
        .replace(/^\s+|\s+$/g, "");
// Remove non-printable and other non-valid JSON characters
    s = s.replace(/[\u0000-\u0019]+/g,"");

    const packageJson = JSON.parse(s);
    // Modify the repository URL
    packageJson.repository.url = gitUrl;
    packageJson.bugs.url = issuesUrl;

    // Write the updated package.json back to file
    fs.writeFileSync(packageJsonPath, JSON.stringify(packageJson, null, 2), 'utf8');
    return 0;
}

console.log("Pre Publish: Update the repo and bugs url to currenly checked out repo, for support in forked repositories.");
// Execute the function
var result = updateRepositoryUrl();

return process.exit(result);