﻿/* File Attacher stuff + ViewModel */
function ModalViewModel() {
    
    var self = this;
    self.currFolderId = ko.observable("");

    var careCenterID = "Center/99"; // HARDCODED Care CenterID

    ko.bindingHandlers.bootstrapModal = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            var props = valueAccessor(),
                vm = bindingContext.createChildContext(viewModel);

            ko.utils.extend(vm, props);

            vm.close = function () {
                vm.show(false);
                vm.onClose();
            };

            vm.action = function () {
                vm.show(false);
                vm.onAction();
            }

            ko.utils.toggleDomNodeCssClass(element, "modal hide fade", true);
            ko.renderTemplate("myModal", vm, null, element);

            var showHide = ko.computed(function () {
                $(element).modal(vm.show() ? 'show' : 'hide');
            });

            return {
                controlsDescendantBindings: true
            };
        }
    }

    self.modal = {
        header: ko.observable("add files"),
        closeLabel: "cancel",
        primaryLabel: "save",
        show: ko.observable(false),

        onClose: function () {
            self.onModalClose();
        },

        onAction: function () {
            self.onModalAction();
        }
    }

    self.fileAttachs = [];

    self.showModal = function (root) {
        //h4ck to set currFolderId from MainViewModel
        self.currFolderId(root.MainViewModel.currFolderId());

        self.modal.show(true);
        /* Bind FineUploader to modal view */
        $('#fine-uploader').fineUploader({
            debug: true,
            request: {
                endpoint: 'S3Web/UploadFile'
            }
        }).on('complete', function (event, id, name, responseJSON) {
            /*
            (event) // jQuery.event
            (id) // index in S3??
            (name) // lemur.jpg
            (responseJSON) // {S3FileName: "240eadf5-ec49-4d02-93ea-a3a5404f28f7", success: true}
            */
            if (responseJSON.success) {
                self.fileAttachs.push(
                    {
                        Guid: '',
                        Key: responseJSON.S3FileName,
                        MimeType : '', // set on server side on save
                        Filename: name,
                        Extension: name.slice(name.indexOf('.'), name.length)
                    });
            } else {
                console.log('There was an error with FineUploader');
            }
        });
    }

    self.onModalClose = function () {
        self.fileAttachs = [];
    }

    self.onModalAction = function () {
        console.log(self.fileAttachs);
        console.log(self.currFolderId());
        var fArr = self.fileAttachs;
        var fID = new String(self.currFolderId());

        console.log(fArr);
        console.log(fID);

        
        $.ajax({
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            url: "/api/v1/FileAtt/SaveUploads",
            data: JSON.stringify({ centerIndex: careCenterID, ID: fID, FileAtts: fArr }), // file guid
            success: function (data) {
                console.log(data);
            },
            error: function (data) {
                console.log(data);
            }
        });

        self.fileAttachs = []; // clear 
    }
}