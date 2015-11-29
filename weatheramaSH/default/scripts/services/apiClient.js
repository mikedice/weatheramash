/**
 * Created by miked on 11/27/2015.
 */
app.service('apiClient', ['$window', '$http', '$log', 'userInfo', function($window, $http, $log, userInfo){
    return {
        beginSignIn: function () {
            // $window must be used because this sign-in needs to bounce off the server.
            $window.location.href = '/oauth/TwitterSignIn';
        },
        getUserInfo: function () {
            $http.get('/api/user').then(function (success) {
                $log.info(success);
                userInfo.setUser(success.data);

            }, function (error) {
                $log.info(success);
            });
        }

    }

}]);
