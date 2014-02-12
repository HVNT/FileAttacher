var viewModel = {};
viewModel.MainViewModel = new MainViewModel();
viewModel.ModalViewModel = new ModalViewModel();

$(document).ready(function () {
    
    var mainContainer = $('#mainContainer')
    var appContent = $('#appContent');
    var tableContainer = $("#table-container");

    var draggableItems = $("#table-item-container");

    /* tooltips for icons */
    $(function () {
        $('.icon-file').tooltip();
        $('.icon-folder-open').tooltip();
        $('.icon-download-alt').tooltip();
    });
    /* disable selection on certain items to enhance UX */
    // aka we don't want shit highlighting everywhere when I click to drag/select etc
    $("ul, li, i").disableSelection();

    /* DRAGGABLE */
    var draggableFiles = $(".files-body");
    var droppableFolders = $(".folders-body");

    $('.table-item-container ul li').on("hover", function () {
        $(this).addClass('highlight-droppables')
    });

    draggableItems.draggable({
        helper: itemDragHelper,
        cursorAt: {
            top: 0,
            left: 0,
            right: 35,
            bottom: 0,
        },
        opacity: .85,
        containment: mainContainer,
    });

    function itemDragHelper() {
        var z = $(event.target).closest('li').find('.name-col');
        return z.hasClass("folder") ? $('<div class="drag-table-item"><i class="icon-folder-close"></i>' + z.text() + '</div>') : 
            $('<div class="drag-table-item"><i class="icon-file"></i>' + z.text() + '</div>');
    };

    /* DROPPABLE */
    draggableItems.droppable({
        activeClass: onPickUp,
        drop: onDrop,
        over: onFolderHover,
        out: onFolderHoverExit,
    });

    function onPickUp() {
        console.log('pickedup!');

        $('.folders-body').removeClass('icon-folder-close');
    }

    function onDrop() {
        console.log('dropped!');
        
        // check if dropped over folder && !this.folder
        // get id of folder

        // get id
        // ajax to ctrl with new folder location and curr file to be moved

        /*
            CONTROLLER: 
                find currLocation of file about to be moved
                set temp to file/folder thats about to be moved
                delete file at currLocation
                find new folder location
                put temp file at this location
                save async()
        */
    }

    function onFolderHover() {
        console.log('hovering!');
    }

    // find closest icon-folder-close and set to open

    function onFolderHoverExit() {
        console.log('exit');

    }

    ko.applyBindings(viewModel);
});




/*
draggableFiles.draggable({
    helper: fileDragHelper,
    cursorAt: {
        top: 0,
        left: 0,
        right: 35,
        bottom: 0,
    },
    opacity: .85,
    containment: mainContainer,
});

function fileDragHelper() {
    var z = $(event.target).closest('li').find('.name-col');
    return $('<div class="drag-table-item"><i class="icon-file"></i>' + z.text() + '</div>');
};


draggableFolders.draggable({
    helper: folderDragHelper,
    cursorAt: {
        top: 0,
        left: 0,
        right: 35,
        bottom: 0,
    },
    opacity: .85,
    containment: mainContainer,
});

function folderDragHelper() {
    var z = $(event.target).closest('li').find('.name-col');
    return $('<div class="drag-table-item"><i class="icon-folder-close"></i>' + z.text() + '</div>');
}
*/