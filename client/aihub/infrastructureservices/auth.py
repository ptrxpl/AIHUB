from aihub.infrastructureservices.base import HttpClient

class AuthClient(HttpClient):
    """
    Client to interact with Experiment api
    """
    def __init__(self):
        self.url = "/users/authenticate"
        super().__init__() # Call __init__() on HttpClient
    
    def authenticate(self, login_data):
        """
        Send request with user, password, get 'token'
        """
       
        response = self.request('POST', self.url, json=login_data.to_dict())       

        return response.json()['token']
