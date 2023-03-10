from marshmallow import Schema, fields
from aihub.domain.base import BaseModel

class LoginSchema(Schema):
    username = fields.Str()
    password = fields.Str()

class LoginData(BaseModel):
    schema = LoginSchema()

    def __init__(self, user, password):
        self.username = user
        self.password = password




        