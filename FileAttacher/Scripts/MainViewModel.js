function MainViewModel() {

    var self = this; // view model
    self.currFolderId = ko.observable(""); // current folder id - set to root on load
    self.files = ko.observableArray(); // container for current folder
    self.folders = ko.observableArray(); // container for current folder
    self.createNewFolder = ko.observable(false); // show new folder input field?
    self.newFolderName = ko.observable(""); // new folder input field text

    //?
    self.folderNav = ko.observableArray(); // for nested folder nav, when empty at root

    var careCenterID = "Center/99"; // HARDCODED Care CenterID

    //ON page load
    $.ajax({
        type: "GET",
        contentType: "application/json",
        dataType: "json",
        url: "/api/v1/Folder/GetFolder?cID=" + careCenterID, //root
        success: function (data) {

            console.log(data);
            
            self.currFolderId(data.g);
            self.files(data.FileAtts);
            self.folders(data.Folders);

            makeDummData();
            // push to current folderNav
            //self.folderNav.push(data.Id);
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
            url: "/api/v1/FileAtt/RemoveFile",
            data: JSON.stringify({cID: careCenterID, fileID: f.g}), // file guid
            success: function (data) {
                console.log(data);

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
                data: JSON.stringify({ cID: careCenterID, folderID: fID, f: folder}),
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
        //self.currFolderId(Id);
        console.log(Id);
        //just open folder..
        /*
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
        */
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

    self.moveItem = function () {


    }

    var makeDummData = function () {
        
        self.files.push(
            { Filename: "purple rhino", MimeType: "image/jpeg", g: "4e95ca3c-9107-462a-a375-109587b219es" },
            { Filename: "blue cow", MimeType: "image/jpeg", g: "1e95da3c-9107-462a-a375-109587b219ed" },
            { Filename: "orange bear", MimeType: "image/jpeg", g: "2e95da3c-9107-462a-a375-101532b219ed" },
            { Filename: "yellow jackets", MimeType: "image/jpeg", g: "4f95dx3c-9107-462a-a375-109587b219zd" },
            { Filename: "green panda", MimeType: "image/jpeg", g: "5e25da3c-9107-462a-a375-109587b219ed" }
        );

        self.folders.push(
            { Filename: "purple", MimeType: "folder", g: "4c95da3c-4327-423v-a375-145687b219ed" },
            { Filename: "blue", MimeType: "folder", g: "1x95da3c-9647-442a-a375-109587b219ed" },
            { Filename: "orange", MimeType: "folder", g: "2v95da3c-9237-462a-a375-109587b219ed" }
        );
    }
}