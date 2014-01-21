function MainViewModel() {

    var self = this;
    self.files = ko.observableArray();

    /* For making trash can appear on hover
    self.deleteEnabled = ko.observable(false);
    self.enableDelete = function () {
        self.deleteEnabled(true);
    }
    self.disableDelete = function () {
        self.deleteEnabled(false);
    }
    */
    $.ajax({
        type: "GET",
        contentType: "application/json",
        dataType: "json",
        url: "/api/FileAtt/GetAll",
        success: function (data) {
            console.log(data);
            self.files(data);
        },
        error: function (data) {
            console.log(data);
        }
    });

    self.removeFile = function (file) {
        console.log(file);

        $.ajax({
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            url: "/api/FileAtt/RemoveFile?Id=" + file.Id,
            success: function (data) {
                console.log(data);
                location.reload();
            },
            error: function (data) {
                console.log(data);
            }
        });
    }
}