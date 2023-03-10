import click
from aihub.domain.auth import AuthSettings
from aihub.application.auth_file_manager import AuthFileManager
from aihub.domain.login import LoginData
from aihub.infrastructureservices.auth import AuthClient
from log import logger as aihub_logger

@click.command()
@click.argument('user', nargs=1)
@click.argument('password', nargs=1)
def login(user, password):
    """
    Allows user to login with his username and password.
    """
    auth_client = AuthClient()
    login_data = LoginData(user, password)
    access_token = auth_client.authenticate(login_data)

    auth_settings = AuthSettings(access_token=access_token)
    AuthFileManager.set_auth(auth_settings) # Create .ai-exp

    auth_settings = AuthFileManager.get_auth()
    access_token = auth_settings['access_token']

    if access_token:
        aihub_logger.info("[INFO] User sucessfully logged in.")

    aihub_logger.debug(f"access_token: {access_token}")

@click.command()
def logout():
    """
    Logout current user.
    """
    AuthFileManager.exists_auth()

    auth_settings = AuthFileManager.get_auth()
    access_token = auth_settings['access_token']

    if not access_token:
        aihub_logger.info("[INFO] User wasn't logged. There is nothing to logout.")

    if access_token:
        auth_settings = AuthSettings(access_token="")
        AuthFileManager.set_auth(auth_settings) # Create .ai-exp
        aihub_logger.info("[INFO] User logged out")