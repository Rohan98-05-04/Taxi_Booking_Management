
document.getElementById("fileInput").addEventListener("change", function () {
    var fileInput = this;
    var file = fileInput.files[0];
    var allowedExtensions = /(\.pdf|\.jpg|\.jpeg|\.png)$/i;
    var maxSizeMB = 1; // 1MB

    if (!allowedExtensions.exec(file.name)) {
        alert("Only PDF, JPG, JPEG, and PNG files are allowed.");
        fileInput.value = '';
        return false;
    }

    if (file.size > maxSizeMB * 1024 * 1024) {
        alert("File size must be less than " + maxSizeMB + "MB.");
        fileInput.value = '';
        return false;
    }

    return true;
});