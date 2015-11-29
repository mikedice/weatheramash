/**
 * Created by miked on 11/27/2015.
 */
app.directive('twitterSignIn', ['apiClient', 'userInfo', function(apiClient, userInfo){
    return{
        restrict: 'A',
        controller: function($scope, $element) {
            $scope.signIn = function(){
                console.log('sign in clicked');
                apiClient.beginSignIn();

            }
            $scope.$on('UserUpdated', function (user) {
                if (userInfo.getUser().ScreenName != null) {
                    $scope.isSignedIn = true;
                }
            });
        }
    }
}]);