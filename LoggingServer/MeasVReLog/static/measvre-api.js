server = window.location.href + "/measvre-api";

// Create a new project.
function CreateProject()
{
    // Get project name from user input
    projectName = document.getElementById("Name").value;

    // Make a request to create a project
    const url = server + "/projects"
    fetch(url, {
        method:"POST",
        headers: {
            "Content-Type": "application/json"
        },

        // Sends Json request to the endpoint in proper format
        body: JSON.stringify({
            name: projectName,
        })
    }).then(response => {
        return response.json();
    }).then(data => {
        //  If error exists in data, there was a server side error
        if (data["error"])
            throw Error("Server Error: " + data["error"]);

        // Display the project name and key if the JSON request is successful
        document.getElementById("projectName").innerHTML = "Project name: " + data["name"];
        document.getElementById("projectKey").innerHTML = "Project key: " + data["key"];

        // Reset HTML page
        document.querySelector("form").reset();
    }).catch(_error => {
        // If there is an error, show an error on the html page and reset the input fields
        document.getElementById("projectName").innerHTML = "Failed to create a project";
        document.querySelector("form").reset();
    });
}

// Delete a project from the database
function DeleteProject()
{
    // Get project key from user input
    projectKey = document.getElementById("deleteKey").value;

    // Make a request to delete the project
    const url = server + "/projects/" + projectKey;
    fetch(url, {
        method:"DELETE"
    }).then(response => {
        if (response.status == 204) {
            document.getElementById("deleteProject").innerHTML = "Deletion succesful";
            return response;
        } else {
            return response.json();
        }
    }).then(data => {
        // If error exists in data, there was an server side error
        if (data["error"])
            throw Error("Server Error: " + data["error"]);
    }).catch(error => {
        console.log(error);
        // If there is an error, display error message on html page
        document.getElementById("deleteProject").innerHTML = "Error: project not deleted."
    })
}

// Download data from a project in a zip file.
function GetProjectData()
{
    // Get project key from user input
    projectKey = document.getElementById("getDataKey").value;

    // Make Fetch request to get data
    const url = server + "/projects/" + projectKey;
    fetch(url, {
        method:"GET"
    }).then(response => {
        filename = response.headers.get("Content-Disposition").split(";")[1].split("=")[1];
        return response.blob();
    }).then(blob => {
        // Check the Browser.
        var isIE = false || !!document.documentMode;
        if (isIE) {
            window.navigator.msSaveBlob(blob, table + ".zip");
        } else {
            var url = window.URL || window.webkitURL;
            var link = url.createObjectURL(blob);
            var newlink = document.createElement("a");
            newlink.download = filename;
            newlink.href = link;
            document.body.appendChild(newlink);
            newlink.click();
            document.body.removeChild(newlink);
        }

        // Display success message after download
        document.getElementById("getData").innerHTML = "The zip file containing data of the project has been downloaded.";
    }).catch(_error => {
        // If there is an error, display an error message on the html page
        document.getElementById("getData").innerHTML = "Error: failed to download the data file";
    });
}
