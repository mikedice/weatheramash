app.directive('userInformation', ['userInfo', function (userInfo) {
    return {
        restrict: 'A',
        controller: function ($scope, $element) {

            if (userInfo != null && userInfo.user != null && userInfo.user.ScreenName != null) {
                $scope.userScreenName = userInfo.user.ScreenName;
            }
            $scope.$on('UserUpdated', function (user) {
                $scope.userScreenName = userInfo.getUser().ScreenName;
            });
        }
    }
}]);