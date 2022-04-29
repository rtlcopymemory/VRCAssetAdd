# VRCAssetAdd

- [For users](#for-users)
- [For creators](#for-creators)
- [Donations](#donations)

## For users
![Apply asset dropdown menu](https://i.imgur.com/eJClbDW.png)  

Drag into your scene the prefab of the asset you want to add to your avatar and your avatar, leave them **unmodified**.

![Imgur](https://i.imgur.com/7rk7Sq5.png)  

Drag and drop, from the scene's Hierarchy, your avatar and the asset you want to add into the Apply From File window.  

Press Apply, select the .json file that was given with the asset (or in any case, that someone made using this tool) and enjoy.  
Once tested, you can delete the Backup duplicates that are automatically created.

## For creators
![Create dropdown menu](https://i.imgur.com/j43iW0x.png)  

Create a prefab of your asset with all your settings (for example, your Phybones or Contact Receivers).  
Drag that prefab in your scene along with the avatar base you want to create the setup for, DO NOT MODIFY THE ORIGINALS.  

![Imgur](https://i.imgur.com/cqTrgj3.png)  

Drag and drop, from the scene's Hierarchy, the **unmodified** avatar and asset (prefab) into the Asset Create window.  
Click `Create Targets`, this will hide your original objects and create a duplicate with which you will work with.

Now start moving all the objects from your asset to their correct position in the target avatar's hierarchy.  
**If you test your avatar by clicking play, you will need to drag and drop the unmodified avatar and asset again __BUT DO NOT CREATE NEW TARGETS__, just drag and drop your __target avatar__ back into the "Target Avatar" field**

Once you're done moving all the components of your asset into the target avatar hierarchy, click `Generate` and save the file wherever you want.  
You will then have to provide this file along with the asset prefab to your clients who only need to follow the short instructions above to add it.

## Donations

If you found this tool useful or appreciate my work, you can select an amount to when getting it from [My Gumroad](https://sesilaso.gumroad.com/).  
Alternatively you can [get me a coffee](https://ko-fi.com/thatonepizza)

## Wanted features for the future
- Animator layer transfer to easily setup toggles too
