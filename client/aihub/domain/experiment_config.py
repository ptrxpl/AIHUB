from marshmallow import Schema, fields
from aihub.domain.base import BaseModel

class ExperimentConfigSchema(Schema):
    name = fields.Str()

class ExperimentConfig(BaseModel):
    schema = ExperimentConfigSchema()

    def __init__(self, name):
        self.name = name