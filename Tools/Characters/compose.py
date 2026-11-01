import bpy, sys, mathutils
U="/tmp/claude-0/dl/mco/x/Modular Character Outfits - Fantasy[Standard]/Exports/FBX (Unity)"
B="/tmp/claude-0/dl/ubc/x/Universal Base Characters[Standard]"
bpy.ops.wm.read_factory_settings(use_empty=True)
def imp(p):
    before=set(bpy.data.objects); bpy.ops.import_scene.fbx(filepath=p)
    new=[o for o in bpy.data.objects if o not in before]
    arm=[o for o in new if o.type=='ARMATURE'][0]
    return arm,[o for o in new if o.type=='MESH']
A,outfit=imp(U+"/Outfits/Male_Peasant.fbx")
S,hero=imp(B+"/Base Characters/Unity/Superhero_Male_FullBody.fbx")
H,hair=imp(B+"/Hairstyles/Rigged to Head Bone/FBX (Unity)/Hair_SimpleParted.fbx")
def bpos(arm,name): return arm.matrix_world @ arm.data.bones[name].head_local
for arm in (A,S,H):
    print(arm.name, 'head', tuple(round(v,3) for v in bpos(arm,'Head')), 'neck', tuple(round(v,3) for v in bpos(arm,'neck_01')), 'pelvis', tuple(round(v,3) for v in bpos(arm,'pelvis')))
import bmesh, os
# cut the hero body down to the head + upper neck
body=[o for o in hero if o.name.startswith('SuperHero')][0]
keep={'Head','neck_01'}
gi={g.index:g.name for g in body.vertex_groups}
bm=bmesh.new(); bm.from_mesh(body.data); dl=bm.verts.layers.deform.verify()
kill=[]
for v in bm.verts:
    w=sum(wt for g,wt in v[dl].items() if gi.get(g) in keep)
    tot=sum(v[dl].values()) or 1
    if w/tot<0.5 or v.co.z<0: pass
    if w/tot<0.5: kill.append(v)
bmesh.ops.delete(bm,geom=kill,context='VERTS'); bm.to_mesh(body.data); bm.free()
for o in hero+hair:
    o.parent=A
    for m in o.modifiers:
        if m.type=='ARMATURE': m.object=A
for arm in (S,H): bpy.data.objects.remove(arm)
body.name='Hanson_Head'
for o in hair: o.name='Hanson_Hair'
print('objects',[(o.name,len(o.data.vertices)) for o in bpy.data.objects if o.type=='MESH'])
bpy.ops.wm.save_as_mainfile(filepath='/tmp/claude-0/bwork/hanson.blend')
