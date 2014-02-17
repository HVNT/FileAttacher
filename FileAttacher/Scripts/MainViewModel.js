function MainViewModel() {

    var careCenterID = "Center/99"; // HARDCODED Care CenterID .. will be in a user profile model??

    var self = this; // view model
    self.currFolderId = ko.observable(""); // current folder id - set to root on load
    self.files = ko.observableArray(); // container for current folder
    self.folders = ko.observableArray(); // container for current folder
    self.createNewFolder = ko.observable(false); // show new folder input field?
    self.newFolderName = ko.observable(""); // new folder input field text

    self.data = ko.observableArray();

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
        self.data.push(folder); // push new 

        self.currFolderId(self.data()[self.data().length - 1].g); // set to guid of rootFolder on load
        self.files(self.data()[self.data().length - 1].FileAtts); // set view files
        self.folders(self.data()[self.data().length - 1].Folders); // set view folders
    }

    
    /////////////////////////////////////////////////////////
    /////////////********** DRAG/DROP ***********////////////
    /////////////////////////////////////////////////////////

    /* TODO :
     *
     * disable selection()
     */
    function dragDrop() {

        /* DECLARATIONS */
        var $self = this;

        /* SETTERS */
        function setDrags() {
            

            var temp = [];
            var curr = [];

            var files = self.files();
            var folders = self.folders();

            ko.utils.arrayForEach(files, function (file) {

                if (file !== undefined) {
                    // send through jquery constructor and set draggable
                    var _f = $('#' + file.g);
                    console.log(_f);

                    if (_f !== undefined) {
                        $(_f).draggable({
                            helper: itemDragHelper,
                            cursorAt: {
                                top: 0,
                                left: 0,
                                right: 0,
                                bottom: 0,
                            },
                            opacity: .85,
                            containment: mainContainer,
                        });
                    }
                }
            });

            ko.utils.arrayForEach(folders, function (folder) {
                var _f = $('#' + folder.g);
                console.log(_f);

                $(_f).draggable({
                    helper: itemDragHelper,
                    cursorAt: {
                        top: 0,
                        left: 0,
                        right: 0,
                        bottom: 0,
                    },
                    opacity: .85,
                    containment: mainContainer,
                });
            });

            /*
            while (temp.length > 0) {
                curr = temp.pop();

                //cast to jquery, droppable all files/folders, then add folders to queue
                if (!(curr[0] === undefined) && curr[0].FileAtts.length > 0) {
                    ko.utils.arrayForEach(curr[0].FileAtts, function (file) {
                        // send through jquery constructor and set draggable
                        $(file).draggable({
                            helper: itemDragHelper,
                            cursorAt: {
                                top: 0,
                                left: 0,
                                right: 0,
                                bottom: 0,
                            },
                            opacity: .85,
                            containment: mainContainer,
                        });
                    });
                }
                
                if (!(curr[0] === undefined) && curr[0].Folders.length > 0) {
                    ko.utils.arrayForEach(curr[0].Folders, function (folder) {
                        temp.push(folder); // push to queue to continue dfs
                        // send through jquery constructor and set draggable
                        $(folder).draggable({
                            helper: itemDragHelper,
                            cursorAt: {
                                top: 0,
                                left: 0,
                                right: 0,
                                bottom: 0,
                            },
                            opacity: .85,
                            containment: mainContainer,
                        });
                    });
                }
            }
            */

            // dfs over array
            // cast to jquery object on each node visit
            // set draggable
            
        }

        function setDrops() {

            // dfs over array
            // cast to jquery object on each node visit
            // set droppable

        }

        /* HELPERS */
        function itemDragHelper() {
            var z = $(event.target).closest('li').find('.name-col');
            return z.hasClass("folder") ? $('<span class="drag-table-item"><i class="icon-folder-close"></i>' + z.text() + '</span>') :
                $('<span class="drag-table-item"><i class="icon-file"></i>' + z.text() + '</span>');
        };

        /* INIT */
        $self.init = function () {
            setDrags();
            setDrops();
        }

        // self.update
        return $self;
    };

    /////////////////////////////////////////////////////////
    //////////////********** ACTIONS ***********/////////////
    /////////////////////////////////////////////////////////

    //ON page load
    self.pageLoad = function () {
        $.ajax({
            type: "GET",
            contentType: "application/json",
            dataType: "json",
            url: "/api/v1/Folder/GetFolder?cID=" + careCenterID, //root
            success: function (data) {
                self.data.push(data); // set data at index 0
                self.currFolderId(data.g); // set to guid of rootFolder on load
                self.files(data.FileAtts); // set view files
                self.folders(data.Folders); // set view folders
                //!
                dragDrop().init();
            },
            error: function (data) {
                console.log(data);
            }
        });
    }
    
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
     * Remove Folder on action trash can icon click
     */
    self.removeFolder = function (folder) {
        var f = folder;

        $.ajax({
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            url: "/api/v1/Folder/RemoveFolder",
            data: JSON.stringify({ centerIndex: careCenterID, folderID: f.g }), // file guid
            success: function (data) {
                console.log(data);
                //remove from view.. give some response like a delay fade to show delete
                self.folders.remove(f);
            },
            error: function (data) {
                console.log(data);
            }
        });
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
                //.. give some response like a delay fade to show delete
                self.files.remove(f);
            },
            error: function (data) {
                console.log(data);
            }
        });
    }

    /*
     * Move file from currFolder to newFolder
     */
    self.moveFile = function (sourceFolderID, fileToMoveID, destFolderID) {

        $.ajax({
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            url: "/api/v1/FileAtt/MoveFile",
            data: JSON.stringify({ // correct structure?
                centerIndex: careCenterID,
                currFolderID: sourceFolderID,
                newfolder: {
                    targetFolderID: destFolderID,
                    FileAtts: [
                        { g: fileToMoveID }
                    ]
                }
            }),
            success: function (data) {
                console.log(data);
                //.. give some response like a delay fade to show delete
                // remove from ui, put at new ui? how can we refresh ui hurr;
            },
            error: function (data) {
                console.log(data);
            }
        });
    }

    self.pageLoad(); // load page
}