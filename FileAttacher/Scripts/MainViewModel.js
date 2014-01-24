function MainViewModel() {

    var self = this;
    self.files = ko.observableArray();
    self.createNewFolder = ko.observable(false);
    self.newFolderName = ko.observable("");

    /* For making trash can appear on hover
    self.deleteEnabled = ko.observable(false);
    self.enableDelete = function () {
        self.deleteEnabled(true);
    }
    self.disableDelete = function () {
        self.deleteEnabled(false);
    }
    */
    $.ajax({
        type: "GET",
        contentType: "application/json",
        dataType: "json",
        url: "/api/v1/FileAtt/GetAll",
        success: function (data) {
            console.log(data);
            self.files(data);
        },
        error: function (data) {
            console.log(data);
        }
    });

    $.ajax({
        type: "GET",
        contentType: "application/json",
        dataType: "json",
        url: "/api/v1/Folder/GetRoot",
        success: function (data) {
            console.log(data);
            self.files(data);
        },
        error: function (data) {
            console.log(data);
        }
    });


    self.removeFile = function (file) {
        var f = file;

        $.ajax({
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            url: "/api/v1/FileAtt/RemoveS3File?f=" + f.Id,
            success: function (data) {
                console.log(data);
                location.reload();
            },
            error: function (data) {
                console.log(data);
            }
        });
    }

    self.showImage = function () {

        $("a#imgBlow").fancybox({
            'type': 'image'
        });
    }

    self.showFolderInput = function () {
        if (self.createNewFolder()) {
            self.createNewFolder(false);
            self.newFolderName(""); //clear input field
        } else {
            self.createNewFolder(true);
        }
    }

    self.createFolder = function () {

        if (!self.newFolderName()) {
            console.log("Folder name cannot be empty.");
        } else {
            var folder = {
                Filename: self.newFolderName(),
                MimeType: "folder",
                FileAtts: [],
                Folders: []
            }
            $.ajax({
                type: "POST",
                contentType: "application/json",
                dataType: "json",
                url: "/api/v1/Folder/SaveFolder",
                data: JSON.stringify(folder),
                success: function (data) {
                    console.log(data);
                },
                error: function (data) {
                    console.log(data);
                }
            });
            //self.files.push(folder);
        }
        self.showFolderInput();
        self.newFolderName("");
    }

    /* tooltips for icons */
    $(function () {
        $('.icon-file').tooltip();
        $('.icon-folder-open').tooltip();
        $('.icon-download-alt').tooltip();
    });
}