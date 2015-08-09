import numpy as np
import matplotlib as mpl
import operator as op

class JumpConfiguration:
    """ Variables describing a particular jump to make. """
    def __init__(self, x_0, x, v_0, dt):
        self.x_0 = x_0
        self.x = x
        self.v_0 = v_0
        self.dt = dt

class Joint:
    def __init__(self, position, mass, x_range, y_range, z_range):
        self.position = position
        self.mass = mass
        self.x_range = x_range
        self.y_range = y_range
        self.z_range = z_range
        self.parentNode = None
        self.childNodes = list()
    
    def setParentNode(self, parent):
        self.parentNode = parent
    
    def addChildNode(self, child):
        if (child is None):
            return
        self.childNodes.append(child)
        child.setParentNode(self)

    def addChildren(self, children):
        map(self.childNodes.append, children)

skeleton =  { \
    "head" : Joint(), \
    "pelvis" : Joint([0,0,0]), \
    "hip" : Joint(), \
    "knee" : Joint(), \
    "ankle" : Joint(), \
    "heel" : Joint(), \
    "toe" : Joint() \
}
skeleton["pelvis"].addChild(skeleton["head"])
skeleton["pelvis"].addChild(skeleton["hip"])
skeleton["hip"].addChild(skeleton["knee"])
skeleton["knee"].addChild(skeleton["ankle"])
skeleton["heel"].addChild(skelegon["toe"])
