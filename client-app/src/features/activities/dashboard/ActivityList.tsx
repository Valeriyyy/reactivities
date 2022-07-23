import { format } from "date-fns";
import { observer } from "mobx-react-lite";
import React, { Fragment } from "react";
import { Item, Label } from "semantic-ui-react";
import { useStore } from "../../../app/stores/store";
import ActivityListItem from "./ActivityListItem";


export default observer(function ActivityList() {
    const { activityStore } = useStore();
    const { groupedActivities } = activityStore;

    return (
        <>
            {groupedActivities.map(([group, activities]) => (
                <Fragment key={group}>                    
                    <Label size='large' color='blue'>
                        {format(new Date(group), 'eeee do MMMM')}
                    </Label>
                    <Item.Group divided>
                        {activities.map(activity => (
                            <ActivityListItem key={activity.id} activity={activity}/>
                        ))}
                    </Item.Group>
                </Fragment>
            ))}
        </>
    );
});