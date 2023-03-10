class BaseModel(object):
    """
    Base for all model classes
    """

    def to_dict(self):
        return self.schema.dump(self)

    @classmethod
    def from_dict(self, dct):
        return self.schema.load(dct)