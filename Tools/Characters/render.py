import bpy, math, sys, json
POSE=json.loads(sys.argv[-1]) if sys.argv[-1].startswith('[') else []
bpy.ops.wm.open_mainfile(filepath='/tmp/claude-0/bwork/hanson.blend')
T='/tmp/claude-0/bwork/tex/'
A=[o for o in bpy.data.objects if o.type=='ARMATURE'][0]
texfor={'MI_Peasant':'T_Hanson_Outfit.png','MI_Regular_Male':'T_Hanson_Skin_Body.png','MI_Superhero_Male':'T_Hanson_Skin_Head.png','MI_Eyes':'T_Hanson_Eyes.png','MI_Hair_1':'T_Hanson_Hair.png'}
def toon(name,img):
    m=bpy.data.materials.new('toon_'+name); m.use_nodes=True; nt=m.node_tree; nt.nodes.clear()
    out=nt.nodes.new('ShaderNodeOutputMaterial'); tx=nt.nodes.new('ShaderNodeTexImage'); tx.image=bpy.data.images.load(T+img)
    d=nt.nodes.new('ShaderNodeBsdfDiffuse'); s2r=nt.nodes.new('ShaderNodeShaderToRGB'); ramp=nt.nodes.new('ShaderNodeValToRGB')
    ramp.color_ramp.interpolation='CONSTANT'; ramp.color_ramp.elements[0].color=(0.55,0.5,0.62,1); ramp.color_ramp.elements[1].position=0.18; ramp.color_ramp.elements[1].color=(1,1,1,1)
    mul=nt.nodes.new('ShaderNodeMix'); mul.data_type='RGBA'; mul.blend_type='MULTIPLY'; mul.inputs['Factor'].default_value=1
    em=nt.nodes.new('ShaderNodeEmission')
    L=nt.links.new; L(d.outputs[0],s2r.inputs[0]); L(s2r.outputs[0],ramp.inputs[0]); L(tx.outputs[0],mul.inputs[6]); L(ramp.outputs[0],mul.inputs[7]); L(mul.outputs[2],em.inputs[0]); L(em.outputs[0],out.inputs[0])
    return m
mats={k:toon(k,v) for k,v in texfor.items()}
ink=bpy.data.materials.new('ink'); ink.use_nodes=True; ink.use_backface_culling=True
nt=ink.node_tree; nt.nodes.clear(); o=nt.nodes.new('ShaderNodeOutputMaterial'); e=nt.nodes.new('ShaderNodeEmission'); e.inputs[0].default_value=(0.05,0.03,0.04,1); nt.links.new(e.outputs[0],o.inputs[0])
for ob in bpy.data.objects:
    if ob.type!='MESH': continue
    for sl in ob.material_slots:
        base=sl.material.name.split('.')[0] if sl.material else ''
        if base in mats: sl.material=mats[base]
        else: print('unmapped',ob.name,base)
    if ob.name!='Eyes':
        ob.data.materials.append(ink); sol=ob.modifiers.new('ink','SOLIDIFY'); sol.thickness=-0.006 if ob.name!='Eyebrows' else -0.001; sol.use_flip_normals=True; sol.material_offset=len(ob.data.materials)-1
# relaxed pose by hand
import mathutils as mu
pb=A.pose.bones
def rot(n,axis,deg):
    b=pb[n]; b.rotation_mode='XYZ'; r=list(b.rotation_euler); r['XYZ'.index(axis)]+=math.radians(deg); b.rotation_euler=r
POSE=globals().get('POSE',[])
for n,ax,d in POSE: rot(n,ax,d)
def swing(n,axis,deg):
    b=pb[n]; bpy.context.view_layer.update()
    hd=b.head.copy(); R=mu.Matrix.Rotation(math.radians(deg),4,axis)
    b.matrix=mu.Matrix.Translation(hd)@R@mu.Matrix.Translation(-hd)@b.matrix
    bpy.context.view_layer.update()
for n,ax,d in [('upperarm_l','Y',globals().get('ARM',68)),('upperarm_r','Y',-globals().get('ARM',68)),('lowerarm_l','Y',-12),('lowerarm_r','Y',12)]: swing(n,ax,d)
pb['Head'].scale=(1.1,1.1,1.1)
bpy.context.scene.frame_set(10)
sc=bpy.context.scene; sc.render.engine='BLENDER_EEVEE'; sc.render.resolution_x=640; sc.render.resolution_y=900
w=bpy.data.worlds.new('w'); w.use_nodes=True; w.node_tree.nodes['Background'].inputs[0].default_value=(0.85,0.8,0.75,1); sc.world=w
sun=bpy.data.objects.new('sun',bpy.data.lights.new('sun','SUN')); sun.data.energy=4; sun.rotation_euler=(math.radians(50),0,math.radians(35)); sc.collection.objects.link(sun)
cam=bpy.data.objects.new('cam',bpy.data.cameras.new('cam')); cam.data.lens=60; sc.collection.objects.link(cam); sc.camera=cam
sc.view_settings.view_transform='Standard'
for i,(ang,dist,z,tz,res) in enumerate([(0,4.2,1.0,0.95,(640,900)),(35,4.2,1.0,0.95,(640,900)),(20,1.3,1.62,1.6,(640,640))]):
    a=math.radians(ang); 
    # character faces -Y in Blender after FBX import? try both via name
    cam.location=(dist*math.sin(a),dist*math.cos(a),z)
    d=mathutils.Vector((0,0,tz))-cam.location if False else None
    import mathutils as mu
    cam.rotation_euler=(mu.Vector((0,0,tz))-mu.Vector(cam.location)).to_track_quat('-Z','Y').to_euler()
    sc.render.resolution_x,sc.render.resolution_y=res
    sc.render.filepath=f'/tmp/claude-0/bwork/prev{i}'+globals().get('TAG','')+'.png'; bpy.ops.render.render(write_still=True)
