var viewModel = {};
viewModel.MainViewModel = new MainViewModel();
viewModel.ModalViewModel = new ModalViewModel();

$(document).ready(function () {
    
    var mainContainer = $('#mainContainer');
    var draggableItems = $("#table-item-container ul");
    var droppableItems = $(".folders-body");

    /* tooltips for icons */
    $(function () {
        $('.icon-file').tooltip();
        $('.icon-folder-open').tooltip();
        $('.icon-download-alt').tooltip();
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
    /* disable selection on certain items to enhance UX */
    // aka we don't want shit highlighting everywhere when I click to drag/select etc
    $("ul, li, i").disableSelection();


    /* DRAGGABLE */
    /*
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
    });*/

    /*
    function itemDragHelper() {
        var z = $(event.target).closest('li').find('.name-col');
        return z.hasClass("folder") ? $('<div class="drag-table-item"><i class="icon-folder-close"></i>' + z.text() + '</div>') : 
            $('<div class="drag-table-item"><i class="icon-file"></i>' + z.text() + '</div>');
    };*/

    function itemDragHelper() {
        var li = $(event.target).closest('li');//.find('.name-col');
        var itemID = li.attr('id');
        var z = li.find('.name-col');
        return z.hasClass("folder") ? $('<div id=' + itemID +' class="drag-table-item"><i class="icon-folder-close"></i>' + z.text() + '</div>') :
            $('<div id=' + itemID + ' class="drag-table-item"><i class="icon-file"></i>' + z.text() + '</div>');
    };

    /* DROPPABLE */
    droppableItems.droppable({
        accept: draggableItems,
        activeClass: onPickUp,
        drop: onDrop,
        over: onFolderHover,
        out: onFolderHoverExit,
    });

    function onPickUp() {
        console.log('pickedup!');

        $('.folders-body').removeClass('icon-folder-close');
    }

    function onDrop(event, ui) {
        console.log(ui.helper);
        
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