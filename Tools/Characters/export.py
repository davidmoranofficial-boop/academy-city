import bpy
bpy.ops.wm.open_mainfile(filepath='/tmp/claude-0/bwork/hanson.blend')
A=[o for o in bpy.data.objects if o.type=='ARMATURE'][0]
A.name='Hanson'
for o in bpy.data.objects: o.select_set(True)
bpy.ops.export_scene.fbx(filepath='/tmp/claude-0/bwork/Hanson.fbx', use_selection=True, object_types={'ARMATURE','MESH'},
  apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', axis_forward='-Z', axis_up='Y', add_leaf_bones=False,
  bake_anim=False, mesh_smooth_type='FACE', path_mode='STRIP', armature_nodetype='NULL', use_armature_deform_only=False)
