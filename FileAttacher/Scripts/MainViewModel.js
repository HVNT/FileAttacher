function MainViewModel() {
    //visible:$root.MainViewModel.folderNav().length > 1,

    var careCenterID = "Center/99"; // HARDCODED Care CenterID .. will be in a user profile model??

    var self = this; // view model
    self.currFolderId = ko.observable(""); // current folder id - set to root on load
    self.files = ko.observableArray(); // container for current folder
    self.folders = ko.observableArray(); // container for current folder
    self.createNewFolder = ko.observable(false); // show new folder input field?
    self.newFolderName = ko.observable(""); // new folder input field text

    self.data = ko.observableArray();
    //self.rootFolder = {}; // NOTE: not set as observable.. set as object literal for iteration
    


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

    self.removeFolder = function (folder) {
        console.log("TODO");
    }

    /*
     * On click : nav back by rootFolder
     */
    self.navBack = function () {

        if (self.data().length <= 1) { // at root .. DO NOT POP ROOT
            // do nothing??
            console.log("..at rootFolder");
        } else {
            self.data.pop(); // pop old
            
            self.currFolderId(self.data()[self.data().length - 1].g);
            self.files(self.data()[self.data().length - 1].FileAtts);
            self.folders(self.data()[self.data().length - 1].Folders);
        }
    }

    /*
     * On click : nav in by rootFolder
     */
    self.navIn = function (folder) {
        console.log(folder);
        self.data.push(folder); // push new 

        self.currFolderId(self.data()[self.data().length - 1].g); // set to guid of rootFolder on load
        self.files(self.data()[self.data().length - 1].FileAtts); // set view files
        self.folders(self.data()[self.data().length - 1].Folders); // set view folders

        // set current folder guid
    }




    /////////////////////////////////////////////////////////
    //////////////********** ACTIONS ***********/////////////
    /////////////////////////////////////////////////////////

    //ON page load
    $.ajax({
        type: "GET",
        contentType: "application/json",
        dataType: "json",
        url: "/api/v1/Folder/GetFolder?cID=" + careCenterID, //root
        success: function (data) {
            console.log(data);
            self.data.push(data); // set data at index 0
            self.currFolderId(data.g); // set to guid of rootFolder on load
            self.files(data.FileAtts); // set view files
            self.folders(data.Folders); // set view folders
        },
        error: function (data) {
            console.log(data);
        }
    });

    /*
     * Save new folder on action click '+' icon
     * Constraint: Folder name cannot be empty || ''
     */
    self.createFolder = function () {
        var fID = self.currFolderId();

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
                data: JSON.stringify({ centerIndex: careCenterID, folderID: fID, newfolder: folder }),
                success: function (data) {
                    console.log(data);
                    self.folders.push(data);
                },
                error: function (data) {
                    console.log(data);
                }
            });
        }

        self.showFolderInput(); // hide new folder input field
        self.newFolderName(""); // clear new folder input field
    }

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
            data: JSON.stringify({ centerIndex: careCenterID, ID: f.g }), // file guid
            success: function (data) {
                console.log(data);
                //remove from view.. give some response like a delay fade to show delete
            },
            error: function (data) {
                console.log(data);
            }
        });
    }
}