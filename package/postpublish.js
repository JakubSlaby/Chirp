const { exec } = require('child_process');
const path = require('path');

const repoPath = path.join(__dirname, '../');
const packageJsonPath = path.join(__dirname, '../package.json');
function revertPackageChanges(absoluteRepositoryPath, relativeFilePath)
{
    const normalizedRepoPath = path.normalize(absoluteRepositoryPath);
    const normalizedFilePath = path.normalize(relativeFilePath);
    
    process.chdir(absoluteRepositoryPath);
    exec(`git checkout -- "${relativeFilePath}"`, (error, stdout, stderr) => {
        if (error) {
            console.error(`Error: ${error.message}`);
            return;
        }
        if (stderr) {
            console.error(`Stderr: ${stderr}`);
            return;
        }
        console.log(`Reverted changes to file: ${normalizedFilePath}`);
        console.log(stdout); 
    });
    
    return 0;
}
console.log("Post publish script: Remove any chanegs to package.json");
var result = revertPackageChanges(repoPath, packageJsonPath);
return process.exit(result);