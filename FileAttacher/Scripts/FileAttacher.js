var viewModel = {};
viewModel.MainViewModel = new MainViewModel();
viewModel.ModalViewModel = new ModalViewModel();

$(document).ready(function () {

    /* DRAGGABLE */
    var table = $('table'); // get table

    table.find('tr td.name-col').bind('mousedown', function () {
        table.disableSelection();
    }).bind('mouseup', function () {
        table.enableSelection()
    });

    table.draggable({
        helper: dragHelper,
        cursorAt: {
            top: 0,
            left: 0,
            right: 20,
            bottom: 0
        },
        opacity: .95,
        containment: table, // contain draggable el to table
    });
    
    function dragHelper(event) {
        return $('<div class="drag-table-item"><table></table></div>').find('table').append($(event.target).closest('td.name-col').clone()).end().appendTo('body');
    };

    /* DROPPABLE */
    $('#Table1#droppable tr').droppable({
        //accept: 'table',
        drop: onDrop,
        over: onHover,
        out: onExit
    });

    function onDrop(event, ui) {
        //console.log(ui.helper[0].innerText);
        
        var target = $(event.target.context);
        console.log(target);

        var folder = $(".folder");

        var folderIcon = $(".folder td i");
        folderIcon.removeClass("icon-folder-open");
        folderIcon.addClass("icon-folder-close");
    }

    function onHover(event, ui) {
        console.log("FUCK");

        var folderIcon = $(".folder td i");
        folderIcon.removeClass("icon-folder-close");
        folderIcon.addClass("icon-folder-open");
    }

    function onExit() {

    }

    ko.applyBindings(viewModel);
});