﻿"use strict";

app.controller('AdminUserProfileController',
    function ($scope, $sce, $rootScope, toastr, userprofile, AdminUserService, CommmonService,
        DTOptionsBuilder, DTColumnDefBuilder) {
        $scope.UserProfile = userprofile.data.Data;

        $scope.trustSrc = function (src) {
            return $sce.trustAsResourceUrl(src);
        }

        // Function check string startwith 'http'
        $scope.checkHTTP = function (input) {
            var lowerStr = (input + "").toLowerCase();
            return lowerStr.indexOf('http') == 0;
        }

        $scope.checkLoadBackedlist = false;
        $scope.getBackedProject = function () {
            if ($scope.checkLoadBackedlist == false) {
                var promise = AdminUserService.getBackedProject($scope.UserProfile.UserName);
                promise.then(
                    function (result) {
                        if (result.data.Status === "success") {
                            $scope.ListBacked = result.data.Data;
                            $scope.checkLoadBackedlist = true;
                        } else {
                            var a = CommmonService.checkError(result.data.Type, $rootScope.BaseUrl);
                            if (a) {
                                $scope.Error = result.data.Message;
                                toastr.error($scope.Error, 'Lỗi');
                            }
                        }
                    }
                 );
            }
        };

        $scope.checkLoadCreatedlist = false;
        $scope.getCreatedProject = function () {
            if ($scope.checkLoadCreatedlist == false) {
                var promise = AdminUserService.getCreatedProject($scope.UserProfile.UserName);
                promise.then(
                    function (result) {
                        if (result.data.Status === "success") {
                            $scope.ListCreated = result.data.Data;
                            $scope.checkLoadCreatedlist = true;
                        } else {
                            var a = CommmonService.checkError(result.data.Type, $rootScope.BaseUrl);
                            if (a) {
                                $scope.Error = result.data.Message;
                                toastr.error($scope.Error, 'Lỗi');
                            }
                        }
                    }
                 );
            }
        };

        //$scope.getBackingDetail = function () {
        //    var promise = AdminUserService.getBackingDetail($scope.UserProfile.UserName);
        //    promise.then(
        //        function (result) {
        //            $scope.BackingDetail = result.data.Data;
        //        }
        //     );
        //};


        // Define table
        $scope.dtOptions = DTOptionsBuilder.newOptions()
        .withDisplayLength(10)
        .withOption('order', [0, 'asc'])
        .withOption('stateSave', true)
        .withBootstrap();

        $scope.dtColumnDefs = [
            DTColumnDefBuilder.newColumnDef(0).notSortable(),
            DTColumnDefBuilder.newColumnDef(-1).notSortable()
        ];

    });