function MainViewModel() {

    var self = this; // view model
    self.currFolderId = ko.observable(""); // current folder id - set to root on load
    self.files = ko.observableArray(); // container for current folder
    self.folders = ko.observableArray(); // container for current folder
    self.createNewFolder = ko.observable(false); // show new folder input field?
    self.newFolderName = ko.observable(""); // new folder input field text
    
    //ON page load
    $.ajax({
        type: "GET",
        contentType: "application/json",
        dataType: "json",
        url: "/api/v1/Folder/GetFolder?id=" + self.currFolderId(), //root
        success: function (data) {
            console.log(data);
            self.currFolderId(data.Id);
            self.files(data.FileAtts);
            self.folders(data.Folders);
        },
        error: function (data) {
            console.log(data);
        }
    });

    /*
     * Remove File on action trash can icon click
     */
    self.removeFile = function (file) {
        var f = file;

        $.ajax({
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            url: "/api/v1/FileAtt/RemoveS3File?f=" + f.Id,
            success: function (data) {
                console.log(data);
                //location.reload();
            },
            error: function (data) {
                console.log(data);
            }
        });
    }

    /*
     * Show image preview on modal on full screen icon click
     */
    self.showImage = function () {
        $("a#imgBlow").fancybox({
            'type': 'image'
        });
    }

    /*
     * New Folder input field toggle
     */
    self.showFolderInput = function () {
        if (self.createNewFolder()) {
            self.createNewFolder(false);
            self.newFolderName(""); //clear input field
        } else {
            self.createNewFolder(true);
        }
    }

    /*
     * Save new folder on action click '+' icon
     * Constraint: Folder name cannot be empty || ''
     */
    self.createFolder = function () {
        var fID = self.currFolderId();
        console.log(fID);

        if (!self.newFolderName()) {
            console.log("Folder name cannot be empty.");
        } else {
            var folder = {
                ParentFolderId: "",
                Filename: self.newFolderName(),
                MimeType: "folder",
                FileAttsIds: [],
                FoldersIds: []
            }
            $.ajax({
                type: "POST",
                contentType: "application/json",
                dataType: "json",
                url: "/api/v1/Folder/SaveFolder?fID=" + fID,
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

    self.zipFolder = function (folder) {
        console.log("TODO");
    }

    self.openFolder = function (folder) {
        self.currFolderId(folder.Id);

        $.ajax({
            type: "GET",
            contentType: "application/json",
            dataType: "json",
            url: "/api/v1/Folder/GetFolder?id=" + self.currFolderId(), //root
            success: function (data) {
                console.log(data);
                self.currFolderId(data.Id);
                self.files(data.FileAtts);
                self.folders(data.Folders);
            },
            error: function (data) {
                console.log(data);
            }
        });
    }

    self.removeFolder = function (folder) {
        console.log("TODO");
    }

    /* tooltips for icons */
    $(function () {
        $('.icon-file').tooltip();
        $('.icon-folder-open').tooltip();
        $('.icon-download-alt').tooltip();
    });

}