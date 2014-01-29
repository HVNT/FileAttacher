function MainViewModel() {

    var self = this; // view model
    self.currFolderId = ko.observable(""); // current folder id - set to root on load
    self.files = ko.observableArray(); // container for current folder
    self.folders = ko.observableArray(); // container for current folder
    self.createNewFolder = ko.observable(false); // show new folder input field?
    self.newFolderName = ko.observable(""); // new folder input field text
    self.folderNav = ko.observableArray(); // for nested folder nav, when empty at root

    //self.showOpen = ko.observable(false); open folder icon v closed on hover


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

            // push to current folderNav
            self.folderNav.push(data.Id);
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

        $("a#imgblow").fancybox({
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
                    self.folders(data);

                },
                error: function (data) {
                    console.log(data);
                }
            });
        }

        self.showFolderInput(); // hide new folder input field
        self.newFolderName(""); // clear new folder input field
    }

    self.zipFolder = function (folder) {
        console.log("TODO");
    }

    // couldnt figure out how to only pass Id as param, used this as 
    // wrapper to make openFolder more usable
    self.openFolderWrap = function (folder) {
        self.openFolder(folder.Id);
    }

    self.openFolder = function (Id) {
        self.currFolderId(Id);

        $.ajax({
            type: "GET",
            contentType: "application/json",
            dataType: "json",
            url: "/api/v1/Folder/GetFolder?id=" + self.currFolderId(), //root
            success: function (data) {

                self.currFolderId(data.Id);
                self.files(data.FileAtts);
                self.folders(data.Folders);
                 
                // push to folderNav to know location
                self.folderNav.push(data.Id);
            },
            error: function (data) {
                console.log(data);
            }
        });

    }

    self.removeFolder = function (folder) {
        console.log("TODO");
    }

    /*
     * On click : nav back by using self.folderNav arr
     */
    self.navBack = function (data) {

        var curr = self.folderNav.pop(); // pop current

        self.openFolder(self.folderNav.pop()); // set to last
    }


    /* tooltips for icons */
    $(function () {
        $('.icon-file').tooltip();
        $('.icon-folder-open').tooltip();
        $('.icon-download-alt').tooltip();
//        $('.icon-fullscreen').tooltip(); //?
        $('.icon-trash').tooltip(); //?
    });

}