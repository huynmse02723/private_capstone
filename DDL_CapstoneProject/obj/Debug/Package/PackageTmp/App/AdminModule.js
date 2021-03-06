﻿"use strict";
var service = angular.module("DDLService", []);
var directive = angular.module("DDLDirective", []);
var app = angular.module("AdminApp", ["ngRoute", "ngAnimate", "ngSanitize", "DDLService",
    "DDLDirective", 'angular-loading-bar', 'textAngular', 'toastr', 'ui.bootstrap', 'monospaced.elastic',
    'datatables', 'datatables.bootstrap', 'oitozero.ngSweetAlert', 'blockUI', 'chart.js', "highcharts-ng", 'ChartAngular']);

// Show Routing.
app.config(["$routeProvider", function ($routeProvider) {
    $routeProvider.when("/dashboard",
        {
            caseInsensitiveMatch: true,
            redirectTo: "/"
        });
    $routeProvider.when("/",
        {
            caseInsensitiveMatch: true,
            templateUrl: "/AdminPartial/Dashboard",
            controller: 'AdminDashBoardController',
            activeTab: 'dashboard',
            breadcrumb: ['Quản lý chung', 'Thống kê'],
            title: 'Thống kê',
            resolve: {
                basicInfo: ['$rootScope', '$q', 'AdminProjectService', 'CommmonService', function ($rootScope, $q, AdminProjectService, CommmonService) {
                    var promise = AdminProjectService.getBasicDashboardInfo();
                    return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
                }],
                listcategory: ['$rootScope', '$q', 'AdminCategoryService', 'CommmonService', function ($rootScope, $q, AdminCategoryService, CommmonService) {
                    var promise = AdminCategoryService.getCategories();
                    return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
                }],
                topBackers: ['$rootScope', '$q', 'AdminUserService', 'CommmonService', function ($rootScope, $q, AdminUserService, CommmonService) {
                    var promise = AdminUserService.getTopBacker();
                    return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
                }],
                statistic: ['$rootScope', '$q', 'AdminProjectService', 'CommmonService', function ($rootScope, $q, AdminProjectService, CommmonService) {
                    var d = new Date();
                    var n = d.getFullYear();
                    var currentYear = parseInt(n);
                    var promise = AdminProjectService.getProjectStatistic(currentYear);
                    return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
                }]
            }
        });

    $routeProvider.when("/reportproject",
        {
            caseInsensitiveMatch: true,
            templateUrl: "/AdminPartial/ReportProject",
            controller: 'AdminReportProjectController',
            activeTab: 'reportproject',
            breadcrumb: ['Quản lý chung', 'Báo xấu dự án'],
            title: 'Báo xấu dự án',
            resolve: {
                listReport: ['$rootScope', '$q', 'AdminReportService', 'CommmonService', function ($rootScope, $q, AdminReportService, CommmonService) {
                    var promise = AdminReportService.GetReportProjects();
                    return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
                }]
            }
        });

    $routeProvider.when("/reportuser",
        {
            caseInsensitiveMatch: true,
            templateUrl: "/AdminPartial/ReportUser",
            controller: 'AdminReportUserController',
            activeTab: 'reportuser',
            breadcrumb: ['Quản lý chung', 'Báo xấu người dùng'],
            title: 'Báo xấu người dùng',
            resolve: {
                listReport: ['$rootScope', '$q', 'AdminReportService', 'CommmonService', function ($rootScope, $q, AdminReportService, CommmonService) {
                    var promise = AdminReportService.GetReportUsers();
                    return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
                }]
            }
        });
    $routeProvider.when("/category",
        {
            caseInsensitiveMatch: true,
            templateUrl: "/AdminPartial/Category",
            controller: 'AdminCategoryController',
            activeTab: 'category',
            breadcrumb: ['Quản lý danh mục', 'Danh sách danh mục'],
            title: 'Quản lý danh mục',
            resolve: {
                listcategory: ['$rootScope', '$q', 'AdminCategoryService', 'CommmonService', function ($rootScope, $q, AdminCategoryService, CommmonService) {
                    var promise = AdminCategoryService.getCategories();
                    return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
                }]
            }
        });
    $routeProvider.when("/project",
        {
            templateUrl: "/AdminPartial/ProjectDashboard",
            controller: 'AdminProjectDashboardController',
            activeTab: 'projectdashboard',
            breadcrumb: ['Quản lí dự án', 'Thông tin chung'],
            title: 'Thông tin chung',
            resolve: {
                listproject: ['$rootScope', '$q', 'AdminProjectService', 'CommmonService', function ($rootScope, $q, AdminProjectService, CommmonService) {
                    var promise = AdminProjectService.getPendingProjectList();
                    return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
                }],
                basicInfo: ['$rootScope', '$q', 'AdminProjectService', 'CommmonService', function ($rootScope, $q, AdminProjectService, CommmonService) {
                    var promise = AdminProjectService.getBasicStatisticProject();
                    return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
                }]
            }
        });
    $routeProvider.when("/project/all",
        {
            templateUrl: "/AdminPartial/ProjectList",
            controller: 'AdminProjectListController',
            activeTab: 'projectList',
            breadcrumb: ['Danh sách dự án', 'Danh sách dự án'],
            title: 'Thông tin chung',
            resolve: {
                listproject: ['$rootScope', '$q', 'AdminProjectService', 'CommmonService', function ($rootScope, $q, AdminProjectService, CommmonService) {
                    var promise = AdminProjectService.getProjectList();
                    return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
                }]
            }
        });
    $routeProvider.when("/project/:code",
       {
           templateUrl: "/AdminPartial/ProjectDetail",
           controller: 'AdminProjectDetailController',
           activeTab: 'projectList',
           breadcrumb: ['Danh sách dự án', 'Chi tiết dự án'],
           title: 'Chi tiết dự án',
           resolve: {
               project: ['$rootScope', '$route', '$q', 'AdminProjectService', 'CommmonService', function ($rootScope, $route, $q, AdminProjectService, CommmonService) {
                   var promise = AdminProjectService.getProjectDetail($route.current.params.code);
                   return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
               }]
           }
       });

    $routeProvider.when("/slide",
        {
            caseInsensitiveMatch: true,
            templateUrl: "/AdminPartial/Slide",
            controller: 'AdminSlideController',
            activeTab: 'slide',
            breadcrumb: ['Quản lý Slide', 'Danh sách Slide'],
            title: 'Quản lý Slide',
            resolve: {
                slides: ['$rootScope', '$q', 'AdminSlideService', 'CommmonService', function ($rootScope, $q, AdminSlideService, CommmonService) {
                    var promise = AdminSlideService.getSlides();
                    return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
                }]
            }
        });
    $routeProvider.when("/message",
        {
            caseInsensitiveMatch: true,
            templateUrl: "/AdminPartial/MessageList",
            controller: 'AdminMessageController',
            activeTab: 'message',
            breadcrumb: ['Tin nhắn', 'Danh sách tin nhắn'],
            title: 'Tin nhắn',
            resolve: {
                conversations: ['$route', '$rootScope', '$q', 'MessageService', 'CommmonService', function ($route, $rootScope, $q, MessageService, CommmonService) {
                    var promise;
                    if ($route.current.params.list === "inbox") {
                        promise = MessageService.getListReceivedConversations();
                    } else if ($route.current.params.list === "sent") {
                        promise = MessageService.getListSentConversations();
                    } else {
                        promise = MessageService.getListConversations();
                    }
                    return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
                }]
            }
        });
    $routeProvider.when("/message/:id",
        {
            caseInsensitiveMatch: true,
            templateUrl: "/AdminPartial/MessageDetail",
            controller: "AdminMessageDetailController",
            activeTab: 'message',
            breadcrumb: ['Tin nhắn', 'Nội dung tin nhắn'],
            title: 'Tin nhắn',
            resolve: {
                conversation: ['$rootScope', '$route', '$q', 'MessageService', 'CommmonService', function ($rootScope, $route, $q, MessageService, CommmonService) {
                    var promise = MessageService.getConversation($route.current.params.id);
                    return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
                }]
            }
        });

    $routeProvider.when("/userlist",
      {
          templateUrl: "/AdminPartial/UserList",
          controller: 'AdminUserListController',
          activeTab: 'userlist',
          breadcrumb: ['Quản lý thành viên', 'Danh sách thành viên'],
          title: 'Danh sách thành viên',
          resolve: {
              listuser: ['$rootScope', '$q', 'AdminUserService', 'CommmonService', function ($rootScope, $q, AdminUserService, CommmonService) {
                  var promise = AdminUserService.getUserlist();
                  return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
              }]
          }
      });

    $routeProvider.when("/userprofile/:username",
      {
          templateUrl: "/AdminPartial/UserProfile",
          controller: 'AdminUserProfileController',
          activeTab: 'userlist',
          breadcrumb: ['Quản lý thành viên', 'Thông tin thành viên'],
          title: 'Thông tin thành viên',
          resolve: {
              userprofile: ['$route', '$rootScope', '$q', 'AdminUserService', 'CommmonService', function ($route, $rootScope, $q, AdminUserService, CommmonService) {
                  var promise = AdminUserService.getUserprofile($route.current.params.username);
                  return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
              }]
          }
      });

    $routeProvider.when("/userdashboard",
      {
          templateUrl: "/AdminPartial/UserDashboard",
          controller: 'AdminUserDashboardController',
          activeTab: 'userdashboard',
          breadcrumb: ['Quản lý thành viên', 'Thông tin chung'],
          title: 'Thông tin chung',
          resolve: {
              userdashboard: ['$route', '$rootScope', '$q', 'AdminUserService', 'CommmonService', function ($route, $rootScope, $q, AdminUserService, CommmonService) {
                  var promise = AdminUserService.getUserDasboard();
                  return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
              }]
          }
      });

    $routeProvider.when("/backingdetail/:id",
       {
           templateUrl: "/AdminPartial/BackingDetail",
           controller: 'AdminBackingDetailController',
           activeTab: 'backinglist',
           breadcrumb: ['Danh sách dự án', 'Chi tiết ủng hộ'],
           title: 'Chi tiết ủng hộ',
           resolve: {
               backing: ['$rootScope', '$route', '$q', 'AdminProjectService', 'CommmonService', function ($rootScope, $route, $q, AdminProjectService, CommmonService) {
                   var promise = AdminProjectService.getBackingDetail($route.current.params.id);
                   return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
               }]
           }
       });

    $routeProvider.when("/backinglist",
 {
     templateUrl: "/AdminPartial/BackingList",
     controller: 'AdminBackingListController',
     activeTab: 'backinglist',
     breadcrumb: ['Bảng điều khiển', 'Danh sách ủng hộ'],
     title: 'Danh sách ủng hộ',
     resolve: {
         listBacking: ['$route', '$rootScope', '$q', 'AdminUserService', 'CommmonService', function ($route, $rootScope, $q, AdminUserService, CommmonService) {
             var promise = AdminUserService.getListBacking();
             return CommmonService.checkHttpResult($q, promise, $rootScope.BaseUrl);
         }]
     }
 });

    $routeProvider.when("/notfound",
        {
            caseInsensitiveMatch: true,
            templateUrl: "/AdminPartial/NotFound",
            activeTab: 'error',
            breadcrumb: ['Quản lí', 'Trang không tìm thấy'],
            title: 'Trang không tìm thấy'
        });
    $routeProvider.when("/error",
        {
            caseInsensitiveMatch: true,
            templateUrl: "/AdminPartial/Error",
            activeTab: 'error',
            breadcrumb: ['Quản lí', 'Lỗi'],
            title: 'Lỗi'
        });

    $routeProvider.otherwise({
        redirectTo: "/",
        activeTab: '',
        breadcrumb: [],
        title: ''
    });

    //$locationProvider.html5Mode(false).hashPrefix("!");
}]).config(['$provide', function ($provide) {
    $provide.decorator('taOptions', ['taRegisterTool', '$delegate', function (taRegisterTool, taOptions) {

        taOptions.forceTextAngularSanitize = false;

        return taOptions;
    }]);
}]).config(['cfpLoadingBarProvider', function (cfpLoadingBarProvider) {
    cfpLoadingBarProvider.includeSpinner = false;
}]).config(['ChartJsProvider', function (ChartJsProvider) {
    //// Configure all charts
    //ChartJsProvider.setOptions({
    //    colours: ['#FF5252', '#FF8A80'],
    //    responsive: false
    //});
    // Configure all line charts
    ChartJsProvider.setOptions('Line', {
        datasetFill: false
    });
}]);

app.run(['$rootScope', '$window', '$anchorScroll', 'DTDefaultOptions', 'toastrConfig', 'blockUIConfig', 'MessageService',
    function ($rootScope, $window, $anchorScroll, DTDefaultOptions, toastrConfig, blockUIConfig, MessageService) {

        $rootScope.$on('$routeChangeError', function (e, curr, prev) {
            e.preventDefault();
        });

        // Scroll top when route change.
        $rootScope.$on("$viewContentLoaded", function () {
            $window.scrollTo(0, 0);
        });

        // Scroll top when route change.
        $rootScope.$on("$routeChangeStart", function (e, curr, prev) {
            if (curr.$$route !== undefined) {
                $rootScope.Page = {
                    title: curr.$$route.title !== undefined ? curr.$$route.title : "",
                    activeTab: curr.$$route.activeTab !== undefined ? curr.$$route.activeTab : "",
                    breadcrumb: curr.$$route.breadcrumb !== undefined ? curr.$$route.breadcrumb : ""
                }
            }
            //        ShareData.currentPage =

        });

        $rootScope.$on('$routeChangeSuccess', function (e, curr, prev) {
            if (curr.$$route.redirectTo === undefined) {
                var promiseGet = MessageService.getNumberNewMessage();
                promiseGet.then(
                    function (result) {
                        if (result.data.Status === "success") {
                            //Save new message number into $rootScope
                            $rootScope.NumberNewMessage = result.data.Data;
                            $rootScope.NumberNewMessage.Total = result.data.Data.ReceivedMessage + result.data.Data.SentMessage;
                        } else {
                            $rootScope.NumberNewMessage.Total = 0;
                            $rootScope.NumberNewMessage.ReceivedMessage = 0;
                            $rootScope.NumberNewMessage.SentMessage = 0;
                        }
                    },
                    function (error) {
                        $rootScope.NumberNewMessage.Total = 0;
                        $rootScope.NumberNewMessage.ReceivedMessage = 0;
                        $rootScope.NumberNewMessage.SentMessage = 0;
                    });
            }
        });

        DTDefaultOptions.setLanguage({
            "sEmptyTable": "Không có dữ liệu",
            "sInfo": "Hiển thị từ _START_ tới _END_ của _TOTAL_",
            "sInfoEmpty": "Hiển thị từ 0 tới 0 của 0",
            "sInfoFiltered": "(Lọc từ tổng số _MAX_ dữ liệu)",
            "sInfoPostFix": "",
            "sInfoThousands": ",",
            "sLengthMenu": "Hiển thị _MENU_",
            "sLoadingRecords": "Đang tải...",
            "sProcessing": "Đang xử lí...",
            "sSearch": "Tìm kiếm:",
            "sZeroRecords": "Không tìm thấy",
            "oPaginate": {
                "sFirst": "Đầu",
                "sLast": "Cuối",
                "sNext": "Tiếp",
                "sPrevious": "Trước"
            },
            "oAria": {
                "sSortAscending": ": activate to sort column ascending",
                "sSortDescending": ": activate to sort column descending"
            }
        });

        // Config angular-blockui.
        blockUIConfig.delay = 100;
        blockUIConfig.blockBrowserNavigation = true;
        // Tell the blockUI service to ignore certain requests
        blockUIConfig.requestFilter = function (config) {
            // If the request starts with '/api/UserApi/GetListUserName' ...
            if (config.url.match(/^\/api\/UserApi\/GetListUserName($|\/).*/)) {
                return false; // ... don't block it.
            }
        };

        // Config angular toarstr.
        angular.extend(toastrConfig, {
            maxOpened: 1,
            closeButton: true,
            newestOnTop: true,
            autoDismiss: true,
            //progressBar: true
        });

        // Base Url of web app.
        $rootScope.BaseUrl = angular.element($('#BaseUrl')).val();

        // Load authen info:
        $rootScope.UserInfo = {
            IsAuthen: false
        };
        // 1. define function
        //function checkLoginStatus() {
        //    var promiseGet = UserService.checkLoginStatus();
        //    promiseGet.then(
        //        function (result) {
        //            if (result.data.Status === "success") {
        //                // Save authen info into $rootScope
        //                $rootScope.UserInfo = result.data.Data;
        //                $rootScope.UserInfo.IsAuthen = true;
        //            } else {
        //                $rootScope.UserInfo = {
        //                    IsAuthen: false
        //                };
        //            }
        //        },
        //        function (error) {
        //            // todo here.
        //        });
        //}
        // 2. call function
        //checkLoginStatus();
    }]);