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

            // update drag/drop
            dragDrop().go();
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

        // update drag/drop
        dragDrop().go();
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
        function set() {

            var files = self.files();
            var folders = self.folders();
            var itemToMove = null;

            if (files.length > 0) {
                ko.utils.arrayForEach(files, function (file) {
                    if (file !== undefined) {
                        var _f = $('#' + file.g);

                        if (_f !== undefined) {
                            $(_f).draggable({
                                helper: function () {
                                    itemToMove = file;
                                    var z = $(event.target).closest('li').find('.name-col');
                                    return z.hasClass("folder") ? $('<span class="drag-table-item"><i class="icon-folder-close"></i>' + z.text() + '</span>') :
                                        $('<span class="drag-table-item"><i class="icon-file"></i>' + z.text() + '</span>');
                                }, cursor: "pointer",
                                cursorAt: { top: 0, left: 0, right: 25, bottom: 0 },
                                opacity: .95,
                                containment: mainContainer,
                            });
                        }
                    }
                });
            }

            if (folders.length > 0) {
                ko.utils.arrayForEach(folders, function (folder) {
                    if (folder !== undefined) {
                        var _f = $('#' + folder.g);

                        //set draggable
                        $(_f).draggable({
                            helper: function () {
                                itemToMove = folder;
                                var z = $(event.target).closest('li').find('.name-col');
                                return z.hasClass("folder") ? $('<span class="drag-table-item"><i class="icon-folder-close"></i>' + z.text() + '</span>') :
                                    $('<span class="drag-table-item"><i class="icon-file"></i>' + z.text() + '</span>');
                            },
                            cursor: "pointer",
                            cursorAt: { top: 0, left: 0, right: 25, bottom: 0 },
                            opacity: .95,
                            containment: mainContainer,
                        });

                        //set droppable
                        $(_f).droppable({
                            //activeClass: onPickUp,
                            drop: function () {
                                if (itemToMove !== null) {
                                    console.log(folder.Filename);
                                    //folder.g = target droppable
                                    self.moveFile(self.currFolderId, itemToMove.g, folder.g);
                                }
                                else {
                                    console.log("itemToMove is null");
                                }
                            },
                            //over: onFolderHover,
                            //out: onFolderHoverExit,
                        });

                        //set hover
                        var _hover = $('#' + folder.g + ' i.icon-folder-close');
                        _hover.mouseenter(function () {
                            $(this).removeClass('icon-folder-close');
                            $(this).addClass('icon-folder-open');
                        }).mouseleave(function () {
                            $(this).addClass('icon-folder-close');
                            $(this).removeClass('icon-folder-open');
                        })
                    }
                });
            }
        }

        /* INIT */
        $self.go = function () {
            itemToMove = null;
            set();
        }

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
                dragDrop().go();
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
                    //!
                    dragDrop().go();
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

        var folder = {
            g: destFolderID,
            MimeType: "folder",
            FileAtts: [
                { g: fileToMoveID }
            ],
            Folders: [],
        }

        $.ajax({
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            url: "/api/v1/Folder/MoveFile",
            data: JSON.stringify({ // correct structure?
                centerIndex: careCenterID,
                folderID: sourceFolderID(),
                newfolder: folder
            }),
            success: function (data) {
                console.log(data);
                //.. give some response like a delay fade to show delete
                // remove from ui, put at new ui? how can we refresh ui hurr;
                var files = self.files();
                console.log(files);
                ko.utils.arrayForEach(files, function (file) {
                    if (file.g == fileToMoveID) {
                        //self.files.remove(file);
                    }
                    else {
                        console.log("I can't find the file to remove from view");
                    }
                });
            },
            error: function (data) {
                console.log(data);
            }
        });
    }

    self.pageLoad(); // load page
}