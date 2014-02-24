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
            $('#newFolderInput').focus();
        }
    }

    /*
     * On click : nav in by rootFolder
     */
    self.navIn = function (folder) {
        if (folder != null) {
            self.data.push(folder); // push new 

            self.currFolderId(self.data()[self.data().length - 1].g); // set to guid of rootFolder on load
            self.files(self.data()[self.data().length - 1].FileAtts); // set view files
            self.folders(self.data()[self.data().length - 1].Folders); // set view folders

            // update drag/drop
            dragDrop().go();
        }
    }

    /*
     * On fileMove 
     */
    self.navInWFile = function (folderDestination, fileMove) {
        if (folderDestination != null && fileMove != null) {
            self.data.push(folderDestination); // push new 
            self.currFolderId(self.data()[self.data().length - 1].g); // set to guid of rootFolder on load
            self.files(self.data()[self.data().length - 1].FileAtts); // set view files
            self.folders(self.data()[self.data().length - 1].Folders); // set view folders
            // push fileMove
            self.files.push(fileMove);
            // update drag/drop
            dragDrop().go();
        }
    }

    /*
     * On folderMove 
     */
    self.navInWFolder = function (folderDestination, folderToMove) {
        if (folderDestination != null && folderToMove != null) {
            self.data.push(folderDestination); // push new 
            self.currFolderId(self.data()[self.data().length - 1].g); // set to guid of rootFolder on load
            self.files(self.data()[self.data().length - 1].FileAtts); // set view files
            self.folders(self.data()[self.data().length - 1].Folders); // set view folders
            // push folderMove
            self.folders.push(folderToMove);
            // update drag/drop
            dragDrop().go();
        }
    }

    /*
     * On click/drop: navTo
     * 
     * This is really an navBack X times, because only option is to go back
     */
    self.navTo = function (folderDestination) {
        if (self.data().length <= 1) { // at root .. DO NOT POP ROOT
            console.log("..at rootFolder");
        } else if (folderDestination != null) {
            self.data.splice(self.data().indexOf(folderDestination) + 1);
            self.currFolderId(self.data()[self.data().length - 1].g); 
            self.files(self.data()[self.data().length - 1].FileAtts);
            self.folders(self.data()[self.data().length - 1].Folders);
            // update drag/drop
            dragDrop().go();
        }
    }

    self.navToWFile = function (folderDestination, fileToMove) {
        if (self.data().length <= 1) {
            console.log("..at rootFolder");
        }
        else {
            self.data.splice(self.data().indexOf(folderDestination) + 1);
            self.currFolderId(self.data()[self.data().length - 1].g); 
            self.files(self.data()[self.data().length - 1].FileAtts);
            self.folders(self.data()[self.data().length - 1].Folders);
            self.files.push(fileToMove); // push fileToMove to new view files
            // update drag/drop
            dragDrop().go();
        }
    }

    self.navToWFolder = function (folderDestination, folderToMove) {
        if (self.data().length <= 1) {
            console.log("..at rootFolder");
        }
        else {
            self.data.splice(self.data().indexOf(folderDestination) + 1);
            self.currFolderId(self.data()[self.data().length - 1].g); 
            self.files(self.data()[self.data().length - 1].FileAtts);
            self.folders(self.data()[self.data().length - 1].Folders);
            self.files.push(folderToMove); // push fileToMove to new view files
            // update drag/drop
            dragDrop().go();
        }
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
        var itemToMove = null;

        /* SETTERS */
        function set() {

            var files = self.files();
            var folders = self.folders();
            var breadcrumbs = self.data();

            if (files.length > 0) {
                ko.utils.arrayForEach(files, function (file) {
                    if (file != null) {
                        var _f = $('#' + file.g);

                        if (_f != null) {
                            _f.draggable({
                                helper: function () {
                                    itemToMove = file;
                                    var z = $(event.target).closest('li').find('.name-col');
                                    return z.hasClass("folder") ? $('<span class="drag-table-item"><i class="icon-folder-close"></i>' + z.text() + '</span>') :
                                        $('<span class="drag-table-item"><i class="icon-file"></i>' + z.text() + '</span>');
                                },
                                cursor: "pointer",
                                cursorAt: { top: 0, left: 0, right: 25, bottom: 0 },
                                opacity: .65,
                                containment: mainContainer,
                            });
                        }
                    }
                });
            }

            if (folders.length > 0) {
                ko.utils.arrayForEach(folders, function (folder) {
                    if (folder != null) {
                        var _f = $('#' + folder.g);

                        //set draggable
                        _f.draggable({
                            helper: function () {
                                itemToMove = folder;
                                var z = $(event.target).closest('li').find('.name-col');
                                return z.hasClass("folder") ? $('<span class="drag-table-item"><i class="icon-folder-close"></i>' + z.text() + '</span>') :
                                    $('<span class="drag-table-item"><i class="icon-file"></i>' + z.text() + '</span>');
                            },
                            cursor: "pointer",
                            cursorAt: { top: 0, left: 0, right: 25, bottom: 0 },
                            opacity: .65,
                            containment: mainContainer,
                        });

                        //set droppable
                        _f.droppable({
                            activeClass: 'droppable-area',
                            drop: function () {
                                if (itemToMove != null) {
                                    if (itemToMove.MimeType === "folder") {
                                        self.moveFolder(self.currFolderId(), itemToMove.g, folder.g);
                                    }
                                    else {
                                        //folder.g = target droppable
                                        self.moveFile(self.currFolderId(), itemToMove.g, folder.g);
                                    }
                                }
                                else {
                                    console.log("itemToMove is null");
                                }
                            },
                            hoverClass: 'droppable-area-hover',
                            tolerance: 'pointer'
                        });

                        //set hover
                        var _hover = $('#' + folder.g + ' .drop-zone');
                        _hover.mouseenter(function () {
                            $('#' + folder.g + ' i.closed').removeClass('icon-folder-close');
                            $('#' + folder.g + ' i.closed').addClass('icon-folder-open');
                        }).mouseleave(function () {
                            $('#' + folder.g + ' i.closed').removeClass('icon-folder-open');
                            $('#' + folder.g + ' i.closed').addClass('icon-folder-close');
                        });
                    }
                });
            }

            if (breadcrumbs.length > 0) {
                var i = 0;
                ko.utils.arrayForEach(breadcrumbs, function (crumb) {
                    if (crumb != null) {
                        //var _f = $('li > span[id=crumb' + i + ']');
                        var _f = $("#breadcrumbs li >span");

                        _f.droppable({
                            activeClass: 'crumb-drop-active',
                            drop: breadcrumbHelper,
                            hoverClass: 'crumb-drop-hover',
                            tolerance: 'pointer'
                        });
                    }
                    i++;
                });
            }
    }

    function breadcrumbHelper(event, ui) {
        var destCrumbID = $(this).attr('id');
        if (itemToMove != null) {
            if (itemToMove.MimeType === "folder") {
                self.moveFolder(self.currFolderId(), itemToMove.g, destCrumbID);
            }
            else {
                self.moveFile(self.currFolderId(), itemToMove.g, destCrumbID);
            }
        }
        else {
            console.log("itemToMove is null");
        }
    }

        /* INIT */
        $self.go = function () {

            itemToMove = null;
            set();
        }

        return $self;
    };

    // mini h4ck to get dragDrop in ModalViewModel scope..
    // in implementation won't matter because dragdrop will be
    // encapsulated independently
    self.dragDrop = dragDrop;

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
    self.createFolder = function (folder) {

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
        if (sourceFolderID != null && fileToMoveID != null && destFolderID != null) {
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
                    folderID: sourceFolderID,
                    newfolder: folder
                }),
                success: function (data) {
                    var files = self.files();
                    var fileToMove = null;
                    ko.utils.arrayForEach(files, function (file) {
                        if (file != null) {
                            if (file.g == fileToMoveID) {
                                // get file
                                fileToMove = file;
                                // remove file here
                                self.files.remove(file);
                            }
                        }
                    });

                    var folders = self.folders();
                    var folderDestination = null;
                    ko.utils.arrayForEach(folders, function (folder) {
                        if (folder.g == destFolderID) {
                            // get destination folder
                            folderDestination = folder;
                        }
                    });

                    if (fileToMove == null || folderDestination == null) {
                        console.log("I can't find the file to move from view or the destination folder");
                    }
                    else {
                        // move file from view and navIn
                        self.navInWFile(folderDestination, fileToMove);
                    }
                },
                error: function (data) {
                    console.log(data);
                }
            });
        }
    }

    /*
     * Move file from currFolder to newFolder
     */
    self.moveFolder = function (sourceFolderID, folderToMoveID, destFolderID) {
        if (sourceFolderID != null && folderToMoveID != null && destFolderID != null) {
            var folder = {
                g: destFolderID,
                MimeType: "folder",
                Folders: [
                    { g: folderToMoveID }
                ],
                folderToMoveID: [],
            }

            $.ajax({
                type: "POST",
                contentType: "application/json",
                dataType: "json",
                url: "/api/v1/Folder/MoveFolder",
                data: JSON.stringify({ // correct structure?
                    centerIndex: careCenterID,
                    folderID: sourceFolderID,
                    newfolder: folder
                }),
                success: function (data) {
                    var folders = self.folders();
                    var folderToMove = null;
                    ko.utils.arrayForEach(folders, function (folder) {
                        if (folder.g == folderToMoveID) {
                            // get file
                            folderToMove = folder;
                            self.folders.remove(folder);
                        }
                    });

                    // need dfs here !
                    var folderDestination = null;
                    ko.utils.arrayForEach(folders, function (f) {
                        if (f.g == destFolderID) {
                            // get destination folder
                            folderDestination = f;
                        }
                    });

                    if (folderToMove == null || folderDestination == null) {
                        console.log("I can't find the folder to move from view or the destination folder");
                    }
                    else {
                        // move file from view and navIn
                        self.navInWFolder(folderDestination, folderToMove);
                    }
                },
                error: function (data) {
                    console.log(data);
                }
            });
        }
    }

    self.pageLoad(); // load page
}